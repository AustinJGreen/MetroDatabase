from flask import Flask, flash, request, render_template
import MySQLdb
from metrodb import MetroDB
from dbconnect import connection
from rform import Query1Form, Query2Form, Query3Form
import os

db = MetroDB('guest', 'guest')
db.connect()
print("Connected to database.")

app = Flask(__name__)
app.secret_key = '*J/2SvTcP$*tccx'
app.config['SESSION_TYPE'] = 'filesystem'

def get_routes():
     # get all routes so we can store in variable and send to query1 form
    routeResult = db.execute_command("SELECT BR.Route_number FROM BUS_ROUTE BR GROUP BY BR.Route_number ORDER BY BR.Route_number")
    routeList = []
    for route in routeResult.fetch_row(maxrows=0):
        routeList.append(int(route[0].strip()))
    return routeList


@app.route('/')
def homepage():
    return render_template('index.html')

@app.route('/query1', methods=['GET'])
def query1():
    routeList=get_routes()
    return render_template('query1.html',routeList=routeList,ds=False)

@app.route('/query1', methods=['POST'])
def handle_query1():
    print("Got POST")
    routeNum = request.form['Route_number']
    baseTable = db.execute_command("SELECT BS.Stop_Name FROM BUS_ROUTE AS BR, BUS_STOP AS BS, BUS_ROUTE_STOPS AS BRS WHERE BRS.Stop_ID = BS.Stop_ID AND BRS.Route_number = BR.Route_number AND BR.Route_number = \'" + str(routeNum) +"\' GROUP BY BS.Stop_Name;")

    stops = baseTable.fetch_row(maxrows=0)
    arr = []
    for stop in stops:
        arr.append(stop[0])

    routeList=get_routes()
    return render_template('query1.html', routeList=routeList,ds=True,stops=arr)

@app.route('/query2')
def handleQuery2():
    form = Query2Form(request.form)
    return render_template('query2.html',form=form)

@app.route('/query3')
def handleQuery3():
    form = Query3Form(request.form)
    return render_template('query3.html',form=form)

if __name__ == '__main__':
    sess.init_app(app)

    app.debug = True
    app.run()
#


#def connection():
#    conn = MySQLdb.connect(host='metrodb.c6yfhb0actbf.us-west-2.rds.amazonaws.com',
#                           user = 'guest',
#                           passwd = 'guest',
#                           db = 'metrodb')
#    c = conn.cursor()
#
#    return c, conn


#GET, PUT, POST, DELETE, etc

#if request.method == 'POST':
#name=request.form['name']
