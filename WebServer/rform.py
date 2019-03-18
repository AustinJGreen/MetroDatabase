from wtforms import Form, TextField, TextAreaField, validators, StringField, SubmitField, SelectField

class Query1Form(Form):
    routeNumber = TextField('RouteNumber:', validators=[validators.required()])

class Query2Form(Form):
    stopA = SelectField('Stop A', choices=80, validators=[validators.required()])
    stopB = TextField('Stop B', choices=80, validators=[validators.required()])

class Query3Form(Form):
    routeNumber = TextField('RouteNumber:', validators=[validators.required()])

