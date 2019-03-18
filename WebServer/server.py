from flask import Flask, flash, request, render_template
from metrodb import MetroDB
import os
import ssl

context = ssl.SSLContext(ssl.PROTOCOL_TLSv1_2)
context.load_cert_chain('cert.pem', 'key.pem')

db = MetroDB('guest', 'guest')
db.connect()
print("Connected to database.")

app = Flask(__name__)
app.secret_key = '*J/2SvTcP$*tccx'
app.config['SESSION_TYPE'] = 'filesystem'

def get_routes():
    routeResult = db.execute_command("SELECT BR.Route_number FROM BUS_ROUTE BR GROUP BY BR.Route_number ORDER BY BR.Route_number")
    routeList = []
    for route in routeResult:
        routeList.append(int(route[0].strip()))
    return routeList

def get_stop_names():
    stopResult = db.execute_command("SELECT Stop_Name FROM BUS_STOP ORDER BY Stop_Name Asc")
    stopList = []
    for stop in stopResult:
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
    routeNum = request.form['Route_number']
    stops = db.execute_command("SELECT BS.Stop_Name FROM BUS_ROUTE AS BR, BUS_STOP AS BS, BUS_ROUTE_STOPS AS BRS WHERE BRS.Stop_ID = BS.Stop_ID AND BRS.Route_number = BR.Route_number AND BR.Route_number = \'" + str(routeNum) +"\' GROUP BY BS.Stop_Name;")

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
    routes = db.execute_command("SELECT BR.Route_number FROM BUS_ROUTE BR WHERE \'" + str(stopA) + "\' IN (SELECT Stop_Name FROM BUS_STOP A JOIN BUS_ROUTE_STOPS B ON A.Stop_ID=B.Stop_ID WHERE B.Route_number=BR.Route_number) AND \'" + str(stopB) + "\' IN (SELECT Stop_Name FROM BUS_STOP A JOIN BUS_ROUTE_STOPS B ON A.Stop_ID=B.Stop_ID WHERE B.Route_number=BR.Route_number) GROUP BY BR.Route_number")
    
    arr = []
    for route in routes:
        arr.append(route[0])

    stopList = get_stop_names()
    return render_template('query2.html',stopList=stopList,ds=True,routes=arr)

if __name__ == '__main__':
    sess.init_app(app)

    app.debug = True
    app.run(ssl_context=None)

