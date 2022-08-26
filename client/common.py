from psycopg2.extras import DictCursor
import psycopg2.extras
import random
import string
from logging import getLogger, FileHandler, DEBUG, Formatter, ERROR
import datetime
from dotenv import load_dotenv
from os import environ as env
from os.path import join, dirname
import traceback


load_dotenv(verbose=True)
load_dotenv(join(dirname(__file__), 'setting.env'))
database_main = (env.get('MAIN_DATABASE'))
database_kakeibo = (env.get('KAKEIBO_DATABASE'))
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


def random_string(x):
    letters = string.ascii_lowercase  # アルファベットのみ
    result_str = '-'.join(random.choice(letters) for i in range(x))
    return result_str


def get_connection():
    """データベースと接続する。"""
    return psycopg2.connect(database_main)


def cookie_table(id):
    with get_connection() as conn:
        conn.autocommit = True
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select* from cookie"
            )
            p = cur.fetchall()
            li = []
            for i in p:
                li.append(i[0])
            while True:
                r = random_string(10)
                if r in li:
                    pass
                else:
                    break
            cur.execute(
                f"insert into cookie(cookie,id) values('{r}','{id}')"
            )
    return r


def ac_id(name, email, password):
    """ログインフォームから呼び出され、アカウントのidを返す。"""
    with get_connection() as conn:
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select id from people where name='{name}' and email='{email}' and password='{password}'"
            )
            p = cur.fetchall()
    if len(p) == 1:
        return p[0][0]


id_list = []


def get_random_string():
    """アカウントid作成 10文字"""
    with get_connection() as conn:
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select* from people"
            )
            for row in cur:
                a = row[0]
                id_list.append(a)  # 全ユーザのidをリストに
            while True:
                result_str = random_string(10)

                if result_str in id_list:
                    pass
                else:
                    break

            return result_str  # ランダムな10文字


def certification(name, email, password) -> bool:
    with get_connection() as conn:
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select* from people where name='{name}' and email='{email}' and password='{password}'"
            )
            p = cur.fetchall()
    if len(p) == 1:
        return True
    else:
        return False


def cer_cookie(account_id) -> bool:
    with get_connection() as conn:
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select* from people"
            )

            p = cur.fetchall()
            for i in p:
                if account_id == i[0]:
                    return True

                else:
                    print('pass')
                    pass
            return False


def get_name(account_id) -> str:
    """アカウントの名前を返す"""
    with get_connection() as conn:
        conn.autocommit = True
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select name from people where id = '{account_id}'"
            )
            p = cur.fetchone()
            if p:
                return p[0]


def account_table(account_id) -> None:
    """ログイン時に呼び出されテーブルと初期値を作成する"""
    with get_connection() as conn:
        conn.autocommit = True
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"create table if not exists {account_id}(id serial,name text,deta text)"
            )



def url_build(sb:str,ss:str):
    if sb == 'home':
        sbb = False
    else:
        sbb = sb.split('-')
        sbs = '.'.join(sbb)
    if ss == '':
        sa = r'/'
    else:
        sa = fr"/{ss.replace('-','/')}"
    if sbb:
        link = fr"https://{sbs}.shuit.net{sa}"
    else:
        link = fr"https://shuit.net{sa}"
    return link


def id_search(id) -> bool:
    with get_connection() as conn:
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select id from people"
            )
            id_list = cur.fetchall()
    for i in id_list:
        if id == i[0]:
            return False
    return True


def dele_cookie():
    with get_connection() as conn:
        conn.autocommit = True
        with conn.cursor(cursor_factory=DictCursor) as cur:
            dt = datetime.date.today()
            date = dt.year + dt.month + dt.day
            cur.execute(
                f"select id from cookie where term < {date}"
            )
            p = cur.fetchall()
            if p:
                li = []
                for i in p:
                    li.append(i[0])
                li = str(li).replace('[','(')
                li = li.replace(']',')')
                print(li)
                logger.info(f"delete cookie id: {li}")
                cur.execute(
                    f"delete from cookie where id in {li}"
                )


def cookie_serch(cookie, description):
    dele_cookie()
    with get_connection() as conn:
        with conn.cursor(cursor_factory=DictCursor) as cur:
            cur.execute(
                f"select id,description from cookie where cookie='{cookie}'"
            )
            try:
                p = cur.fetchall()[0]
                if p:
                    if p[1] == description:
                        return p[0]
            except :
                logger.error(traceback.format_exc())
                return  False
            return False


def cookie_delete(id):
    try:
        with get_connection() as conn:
            conn.autocommit = True
            with conn.cursor(cursor_factory=DictCursor) as cur:
                cur.execute(
                    f"delete from cookie where id='{id}'"
                )
    except:
        pass