# coding: utf-8
from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.schema import Column
from sqlalchemy.types import Integer, String
from sqlalchemy.orm import sessionmaker


engine = create_engine("postgresql://postgres:vSxySxVaeNMV0E3duJH4yXmRXRmjZNvB"
    "@localhost:5433/test_web")
Base = declarative_base(bind=engine)
"""
SessionClass = sessionmaker(engine)
session = SessionClass()
"""



class User(Base):
    __tablename__ = 'people'
    __table_args__ = {"autoload": True}
    id = Column(String(20), primary_key=True)
    name = Column(String(20), nullable=False)
    email = Column(String(40))
    password = Column(String(40), nullable=False)


class Cookie(Base):
    __tablename__ = 'cookie'
    __table_args__ = {"autoload": True}
    cookie = Column(String(5), primary_key=True)
    id = Column(String(10), nullable=True)
    description = Column(String, nullable=False)
    term = Column(Integer, nullable=False)


def GetUserInfo(CookieId):
    SessionClass = sessionmaker(engine)
    session = SessionClass()

    userId = session.query(Cookie.id).filter(Cookie.cookie == CookieId, Cookie.term < int(DateNow)).first()
    user: User = session.query(User).filter(User.id == userId[0]).first()
    session.close()
    return user

