import websockets
import asyncio
import pickle
import subprocess as ps
from time import sleep
from dotenv import load_dotenv
from os import environ as env
from os.path import join, dirname
from logging import getLogger, FileHandler, DEBUG, Formatter, ERROR
import traceback
import ast


load_dotenv(verbose=True)
load_dotenv(join(dirname(__file__), 'setting.env'))
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


with open("user/deta", "rb") as f:
    deta = pickle.load(f)
user = deta["user"]
password = deta["password"]



async def main():
    async for websocket in websockets.connect("ws://localhost:8765"):
        try:
            a = "{"+"'user':'"+user+"', 'func': 'new', 'deta': {'password': "+"'"+password+"'"+"}}"
            websocket.send()
            async for mes in websocket:
                print(mes)
                if type(mes) == str:
                    mes = ast.literal_eval(mes)
                else:
                    with open("files/none.aaa")
        except websockets.ConnectionClosed:
            continue


if __name__ == "__main__":
    asyncio.run(main())