from websocket_server import WebsocketServer
import logging
import subprocess as process
from psycopg2.extras import DictCursor
import psycopg2.extras
from dotenv import load_dotenv
from os import environ as env
from os.path import join, dirname
from logging import getLogger, FileHandler, DEBUG, Formatter, ERROR
import traceback
import ast


load_dotenv(verbose=True)
load_dotenv(join(dirname(__file__), 'setting.env'))
database_main = (env.get('MAIN_DATABASE'))
logger = getLogger(__name__)
formatter = Formatter("[%(levelname)s] %(asctime)s - %(message)s (%(filename)s)")
handl = FileHandler(env.get('MAIN_LOG'))
handl.setLevel(DEBUG)
handl.setFormatter(formatter)
error_handl = FileHandler(env.get('ERROR_LOG'))
error_handl.setLevel(ERROR)
error_handl.setFormatter(formatter)
logger.setLevel(DEBUG)
logger.addHandler(handl)
logger.addHandler(error_handl)


wait_user = [{"user":"user_id","handler":"client['handler]"}]


def get_connection():
    """データベースと接続する。"""
    return psycopg2.connect(database_main)


def new(client, user, password, server):
    with get_connection() as conn:
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select deta from {user} where name='service'"
            )
            a:dict = cur.fetchall()
    dic = []
    for i in a:
        dic.append(i[0])
    if "share" in dic:
        wait_user.append({"user":user, "handler":client["handler"]})
        return "True"
    else:
        with get_connection() as conn:
            with conn.cursor(cursor_factory=DictCursor) as cur:
                cur.execute(
                    f"select name from people where id='{user}'"
                )
                name = cur.fetchone()
                process.run(["sudo", "useradd", f"{name}_{user}"])
                cur.execute(
                    f"insert into {user}(name,deta) values('service','share')"
                )
        wait_user.append({"user":user, "handler":client["handler"]})
        return "Wellcome"


def upload(server:WebsocketServer, user:str, deta:dict):
    """_summary_

    Args:
        deta (dict): {
            "path":"file_path" #relative path
        }
    """
    print(deta)
    path = deta["path"].strip("/") #相対パス
    with get_connection() as conn:
            with conn.cursor(cursor_factory=DictCursor) as cur:
                cur.execute(
                    f"select deta from {user} where name='sh_path'"
                )
                sh_path = cur.fetchall()
                for p in sh_path:
                    p = p.strip(":")
                    if path == p[0]:
                        path = p[1]
    client = None
    for i in wait_user:
        if user == i["user"]:
            client = {"handler": i["handler"]}
    try:
        if client:
            server.send_message(client, deta["path"])
            return "success"
        logger.error(traceback.format_exc())
        return "error"
    except:
        logger.error(traceback.format_exc())
        return "error"


def edit():
    pass


func = {
    "new": new,
    "upload": upload,
    "edit": edit
}


class Websocket_Server():

    def __init__(self, port):
        self.server = WebsocketServer(host="0.0.0.0",port=port, loglevel=logging.DEBUG)

    # クライアント接続時に呼ばれる関数
    def new_client(self, client, server:WebsocketServer):
        print("new client connected and was given id {}".format(client['id']))

    # クライアント切断時に呼ばれる関数
    def client_left(self, client, server:WebsocketServer):
        print("client({}) disconnected".format(client['id']))

    # ク2ライアントからメッセージを受信したときに呼ばれる関数
    def message_received(self, client, server:WebsocketServer, message):
        """_summary_

        Args:
            client (_type_): _description_
            server (_type_): _description_
            message (dict): {
                                "user": "user_id",
                                "func": "func",
                                "deta": dict
            }
        """
        print("<<<<   " + message)
        #logger.info("<<<<   " + message)
        message = ast.literal_eval(message)
        if message["func"] in func.keys():
            if message["func"] == "new":
                result = new(client,message["user"],message["deta"]["password"], server=server)
            elif message["func"] == "upload":
                result = upload(server=server, user=message["user"], deta=message["deta"])
            else:
                result = func[message["func"]](user=message["user"], deta=message["deta"])
            self.server.send_message(client, result)

    # サーバーを起動する
    def run(self):
        # クライアント接続時のコールバック関数にself.new_client関数をセット
        self.server.set_fn_new_client(self.new_client)
        # クライアント切断時のコールバック関数にself.client_left関数をセット
        self.server.set_fn_client_left(self.client_left)
    # メッセージ受信時のコールバック関数にself.message_received関数をセット
        self.server.set_fn_message_received(self.message_received)
        self.server.run_forever()


PORT=8765 # ポートを指定
ws_server = Websocket_Server(port=PORT)
ws_server.run()