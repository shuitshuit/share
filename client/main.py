from flask import Flask, request, abort, redirect, url_for, render_template
from os import environ as env
from os.path import join, dirname
from dotenv import load_dotenv
from werkzeug.utils import secure_filename
from functools import wraps
from common import cookie_serch
import datetime
import websocket


load_dotenv(verbose=True)
load_dotenv(join(dirname(__file__), 'setting.env'))

application = Flask(__name__)
application.config['UPLOAD_FOLDER'] = env.get("upload_folder")
ALLOWED_EXTENSIONS = set(['png', 'jpg', 'gif','txt','jpeg','zip','mp4','mp3','flac','exe','wav'])
not_login = r"https://shuit.net/login?sb=share"


class User():
    def __init__(self, user_id:str,result:bool):
        self.id:str = user_id
        self.res:bool = result


def login_req():
    cookie = request.cookies.get('user')
    print(cookie)
    if cookie:
        user_id = cookie_serch(cookie,request.environ.get('HTTP_USER_AGENT'))
        print(user_id,565)
        if user_id:
            user = User(user_id, True)
            return user
    user = User(None,False)
    return user



def login(func):
    @wraps(func)
    def wrapper(*args, **kwargs):
        cookie = request.cookies.get('user') #cookie取得
        if cookie:
            user_id = cookie_serch(cookie,request.environ.get('HTTP_USER_AGENT'))
            if user_id:
                user = User(user_id,True)
                return func(user,*args,**kwargs)
        f = str(func).split(' ')[1] # <function ** 0000000>の**を切り取る **はデコレート元の関数名
        link = url_for(f)
        link = link[1:].replace('/','-') #/**/の二文字目以降の/を-に置き換え
        return redirect(f"{not_login}&ss={link}") #ログインしてないのでログインページにリダイレクト
    return wrapper


def allwed_file(filename):
    # .があるかどうかのチェックと、拡張子の確認
    # OKなら１、だめなら0
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS


@application.route("/file",methods=['POST'])
def file():
    user = User("lhbzjdplou",True)
    """user: User = login_req()
    if not user.res:
        return abort(400)"""
    if request.method == "POST":
        if "file" not in request.files:
            print("file is null")
            return abort(400)
        file = request.files['file']
        if file.filename == '':
            print("file name is null")
            return abort(400)
            # ファイルのチェック
        if file and allwed_file(file.filename):
            # 危険な文字を削除（サニタイズ処理)
            name = file.filename.split(".")
            if name[0].isascii():
                filename = secure_filename(file.filename)
            else:
                dt = datetime.datetime.now()
                filename = f"{dt.year}{dt.month}{dt.day}{dt.hour}{dt.minute}{dt.second}.{name[1]}"
            print(filename)
            file.save(f"{application.config['UPLOAD_FOLDER']}/{user.id}/{filename}.aaa")
            ws = websocket.WebSocket()
            ws.connect("ws://localhost:8765/")
            a = "{" + f"'user': '{user.id}','func': 'upload', 'deta': " + "{" + f"'path': '{filename}.aaa'"+"}}"
            ws.send(f"{a}")
            print("<<<<<    "+ws.recv())
            ws.close()
            return redirect(url_for("upload"))
    elif request.method == "GET":
        return redirect(url_for("upload"))


@application.route("/upload",methods=["GET"])
def upload():
    return render_template("index.html")