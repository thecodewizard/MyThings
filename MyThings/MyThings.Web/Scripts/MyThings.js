//This is the general Javascript file
    //The general namespace to avoid conflicts
var MyThings = MyThings || {};

MyThings = {
    //TODO: Remove these comment lines.
    //Here will be the main methods of the javascript
    //This is required in order to natively work with the objects.
    //These are the objects available:
    /* Sensor
    *  Container
    *  Group
    */
    //These objects can be used on two ways:
    // 1. var sensor = new Sensor().loadFromJson(jsonString);
    // 2. var sensor = new Sensor().load(sensorId, callbackFunction);


    //TODO: Add Main Javascript functions here
};

function Sensor(id, company, macaddress, location, creationdate, sensorentries, basestationlat, basestationlng, containers) {
    this.id = id;
    this.company = company;
    this.macaddress = macaddress;
    this.location = location;
    this.creationdate = creationdate;
    this.sensorentries = sensorentries;
    this.basestationlat = basestationlat;
    this.basestationlng = basestationlng;
    this.containers = containers;
    this.load = function (sensorId, callback) {
        callback(new Sensor(1, 2, 3, 4, 5, 6, 7, 8, 9));
    }
    this.loadFromJson = function(json) {
        return new Sensor(1, 2, 3, 4, 5, 6, 7, 8, 9);
    }
}

function Container(id, name, creationtime, value) {
    this.id = id;
    this.name = name;
    this.creationtime = creationtime;
    this.value = value;
    this.load = function (containerId, callback) {
        callback(new Sensor(1, 2, 3, 4, 5, 6, 7, 8, 9));
    }
    this.loadFromJson = function (json) {
        return new Sensor(1, 2, 3, 4, 5, 6, 7, 8, 9);
    }
}

function Group(id, name, sensors) {
    this.id = id;
    this.name = name;
    this.sensors = sensors;
    this.load = function (GroupId, callback) {
        callback(new Sensor(1, 2, 3, 4, 5, 6, 7, 8, 9));
    }
    this.loadFromJson = function (json) {
        return new Sensor(1, 2, 3, 4, 5, 6, 7, 8, 9);
    }
}