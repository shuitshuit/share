import asyncio
from venv import create
import websockets
from logging import getLogger, FileHandler, DEBUG, Formatter, ERROR
import traceback
from os import environ as env
from os.path import join, dirname
from dotenv import load_dotenv
from psycopg2.extras import DictCursor
import psycopg2.extras
import ast
import pickle
import io
import subprocess as ps


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
CONNECTIONS = {} # 接続中のクライエント
SERVERS = {} #接続待機のサーバー


def get_connection():
    """データベースと接続する。"""
    return psycopg2.connect(database_main)


def new(websocket, user:str, password:str):
    SERVERS[user] = websocket
    return "welcome"

async def upload(user:str, deta:dict):
    """_summary_

    Args:
        websocket: ###
        user (str): user id
        deta (dict):    {
                            "path":str
                        }
    """
    print("servers",SERVERS)
    if user in SERVERS.keys():
        socket = SERVERS[user] # サーバーのインスタンス
    else:
        print("cannot access your server.")
        return "cannot access your server."
    try:
        if socket:
            with open(f"upload_foldoer/{user}/{deta['path']}") as f:
                d = io.BytesIO(f.read)
                socket.send(d)
                socket.send("{'path':'"+deta['path']+"'}")
            return "success"
        logger.error(traceback.format_exc())
        return "your server error"
    except:
        logger.error(traceback.format_exc())
        return "error"



func = {
    "new": new,
    "upload": upload
}



async def hello(websocket):
    """_summary_

    Args:
        mes(str): {
            'user': user id(str),
            'func': function(str),
            'deta':{
                detas
            }
        }
    """
    async for mes in websocket:
        #mes = await websocket.recv()
        print(f"<<< {mes}")
        mes = ast.literal_eval(mes)
        CONNECTIONS[mes["user"]] = websocket
        print(CONNECTIONS)
        print(SERVERS)
        if mes["func"] in func.keys():
            if mes["func"] == "new":
                result = new(websocket,mes["user"],mes["deta"]["password"])
            elif mes["func"] == "upload":
                result = await upload(user=mes["user"], deta=mes["deta"])
            else:
                result = await func[mes["func"]](user=mes["user"], deta=mes["deta"])
        print(">>> "+result)
        await websocket.send(str(result))


async def main():
    async with websockets.serve(hello, "localhost", 8765):
        await asyncio.Future()  # run forever

if __name__ == "__main__":
    asyncio.run(main())