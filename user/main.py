from dotenv import load_dotenv
from os import environ as env
from os.path import join, dirname
from logging import getLogger, FileHandler, DEBUG, Formatter, ERROR
import traceback
from websocket import WebSocketApp
import pickle
import subprocess as ps


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


def on_open(ws):
    ws.send(
        "{"+"'user':'"+user+"', 'func': 'new', 'deta': {'password': "+"'"+password+"'"+"}}"
    )


def on_message(ws:WebSocketApp, message):
    print(message)


def on_close(ws, status_code, message):
    ps.run(["python", __file__])


def on_error(ws, error):
    print(traceback.format_exc())
    logger.error(traceback.format_exc())


if __name__ == '__main__':
    wsapp = WebSocketApp("ws://localhost:8765", on_message=on_message, on_open=on_open, on_error=on_error, on_close=on_close)
    wsapp.run_forever()