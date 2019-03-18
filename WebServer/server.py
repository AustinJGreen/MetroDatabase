from flask import Flask, flash, request, render_template
from metrodb import MetroDB
from dbconnect import connection
import os

db = MetroDB('guest', 'guest')
db.connect()
print("Connected to database.")

app = Flask(__name__)
app.secret_key = '*J/2SvTcP$*tccx'
app.config['SESSION_TYPE'] = 'filesystem'

def get_routes():
    routeResult = db.execute_command("SELECT BR.Route_number FROM BUS_ROUTE BR GROUP BY BR.Route_number ORDER BY BR.Route_number")
    routeList = []
    for route in routeResult.fetch_row(maxrows=0):
        routeList.append(int(route[0].strip()))
    return routeList

def get_stop_names():
    stopResult = db.execute_command("SELECT Stop_Name FROM BUS_STOP ORDER BY Stop_Name Asc")
    stopList = []
    for stop in stopResult.fetch_row(maxrows=0):
        stopList.append(stop[0])
    return stopList

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

@app.route('/query2', methods=['GET'])
def query2():
    stopList = get_stop_names()
    return render_template('query2.html',stopList=stopList,ds=False)

@app.route('/query2', methods=['POST'])
def handle_query2():
    stopA = request.form['stop_a']
    stopB = request.form['stop_b']
    baseTable = db.execute_command("SELECT BR.Route_number FROM BUS_ROUTE_STOPS BRS INNER JOIN BUS_ROUTE BR ON BRS.Route_number = BR.Route_number INNER JOIN BUS_STOP BS ON BS.Stop_ID = BRS.Stop_ID WHERE BS.Stop_Name = \'" + str(stopA) + "\' OR BS.Stop_Name = \'" + str(stopB) + "\' GROUP BY BR.Route_number;")

    routes = baseTable.fetch_row(maxrows=0)
    arr = []
    for route in routes:
        arr.append(route[0])

    stopList = get_stop_names()
    return render_template('query2.html',stopList=stopList,ds=True,routes=arr)

if __name__ == '__main__':
    sess.init_app(app)

    app.debug = True
    app.run()

