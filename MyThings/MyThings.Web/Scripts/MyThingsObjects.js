﻿//This is the javascript file with the objects
    //Always load this AFTER jquery and BEFORE other self made scripts

//Serverlocations
var apiBaseUrl = "http://localhost:22056/api/";
var siteBaseUrl = "http://http://localhost:16964/Dashboard/";

//OBJECTS
function Sensor(id, name, company, macaddress, location, creationdate, sensorentries, basestationlat, basestationlng, containers) {
    //Fields
    this.id = id;
    this.name = name;
    this.company = company;
    this.macaddress = macaddress;
    this.location = location;
    this.creationdate = creationdate;
    this.sensorentries = sensorentries;
    this.basestationlat = basestationlat;
    this.basestationlng = basestationlng;
    this.containers = containers;

    //Make an internally callable object
    var that = this;

    //Functions //TODO: Update API URL's
    this.update = function (onSensorUpdated) {
        //send to url in api/get/getSensor/id
        //onSensorUpdated is a function which expects a sensorobject as argument.

        if ($.isFunction(onSensorUpdated)) {
            onSensorUpdated(that);
        }
    }
    this.pin = function (onSensorPinned) {
        //send to url in api/post/pinSensor/id
        //onSensorPinned is a function which expects a sensorobject as argument

        if ($.isFunction(onSensorPinned)) {
            onSensorPinned(that);
        }
    }
}

function Container(id, name, creationtime, sensor, value, valuetime) {
    //Fields
    this.id = id;
    this.name = name;
    this.creationtime = creationtime;
    this.sensor = sensor;
    this.value = value;
    this.valuetime = valuetime;

    //Make an interally callable object
    var that = this;

    //Functions //TODO: Update API URL's
    this.loadCurrentValue = function(onCurrentValueLoaded) {
        //Load the last value of this container in the database
        //onCurrentValueLoaded is a function which expects a containerobject as argument
        if (!$.isFunction(onCurrentValueLoaded)) return;

        if (that.id != null && that.name != null) {
            $.ajax({
                url: apiBaseUrl + "get/getvalue?sensorId=" + that.sensor.id + "&containerId=" + that.id,
                    method: "GET"
                })
                .done(function(json) {
                    if (json != null) {
                        var data = JSON.parse(json);

                        that.value = data.Value;
                        that.valuetime = data.ValueTime;

                        onCurrentValueLoaded(that);
                    }
                }).fail(function() {
                    //If the value couldn't be loaded, send back to user
                    if ($.isFunction(MyThings.logToUser)) {
                        MyThings.logToUser("The value of container nr." + that.id + " (" + that.name + ") could not be loaded.");
                    }
                });
        }
    }
    this.update = function (onContainerUpdated) {
        //send to url in api/get/getContainer/id
        //onContainerUpdated is a function which expects a containerobject as argument.

        if ($.isFunction(onContainerUpdated)) {
            onContainerUpdated(that);
        }
    }
    this.pin = function (onContainerPinned) {
        //send to url in api/post/pinContainer/id
        //onContainerPinned is a function which expects a containerobject as argument

        if ($.isFunction(onContainerPinned)) {
            onContainerPinned(that);
        }
    }
}

function Group(id, name, sensors) {
    //Fields
    this.id = id;
    this.name = name;
    this.sensors = sensors;

    //Make an internally callable object
    var that = this;

    //Functions //TODO: Update API URL's
    this.save = function(onGroupSaved) {
        //send to url in api/post/createGroup/json
        //make json object of 'that' object and pass it as parameter
        //onGroupSaved is a function which expects a groupobject as argument

        if ($.isFunction(onGroupSaved)) {
            onGroupSaved(that);
        }
    }
    this.update = function (onGroupUpdated) {
        //send to url in api/get/getGroup/id
        //onGroupUpdated is a function which expects a groupobject as argument
        
        if ($.isFunction(onGroupUpdated)) {
            onGroupUpdated(that);
        }
    }
    this.pin = function (onGroupPinned) {
        //send to url in api/post/pinGroup/id
        //onGroupPinned is a function which expects a groupobject as argument

        if ($.isFunction(onGroupPinned)) {
            onGroupPinned(that);
        }
    }
    this.addSensor = function(sensor, onGroupSaved) {
        //send to url in api/post/addToGroup/sensor

        //Send object changes to database
        that.save(onGroupSaved);
    }
    this.removeSensor = function(sensor, onGroupSaved) {
        //send to url in api/post/removeFromGroup/sensor
        
        //Send object changes to database
        that.save(onGroupSaved);
    }
    this.hasSensor = function(sensor) {
        //check in the that.sensors whether the given sensor is found.
        //return boolean
    }
}

//STATIC CREATORS
Sensor.load = function (sensorId, onSensorLoaded, loadContainerValues, onContainerValueLoaded) {
    if ($.isFunction(onSensorLoaded)) {
        if (sensorId != null) {
            $.ajax({
                url: apiBaseUrl + "get/getSensor?sensorId=" + sensorId,
                method: "GET",
                contentType: "application/json"
            }).done(function (json) {
                if (json != null) {
                    //Return sensor
                    var sensor = Sensor.loadFromJson(json);
                    onSensorLoaded(sensor);

                    //Load Containervalues if requested
                    loadContainerValues = loadContainerValues || false;
                    if (loadContainerValues == true && $.isFunction(onContainerValueLoaded)) {
                        for (var i = 0; i < sensor.containers.length; i++) {
                            var container = sensor.containers[i];
                            container.loadCurrentValue(onContainerValueLoaded);
                        }
                    }
                }
            }).fail(function () {
                //If the sensor couldn't be loaded, send back to user
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("Sensor nr." + sensorId + " could not be loaded.");
                }
            });
        }
        onSensorLoaded(null);
    }
    return null;
};
Sensor.loadMany = function (count, onSensorLoaded, loadContainerValues, onContainerValueLoaded) {
    if ($.isFunction(onSensorLoaded)) {
        if (count != null) {
            $.ajax({
                url: apiBaseUrl + "get/getsensors?count=" + count,
                method: "GET",
                contentType: "application/json"
            }).done(function (json) {
                if (json != null) {
                    //Parse the list of sensors
                    var sensors = JSON.parse(json);
                    if (sensors != null && sensors.length > 0) {
                        for (var ii = 0; ii < sensors.length; ii++) {
                            //Return sensor
                            var sensorjson = JSON.stringify(sensors[ii]);
                            var sensor = Sensor.loadFromJson(sensorjson);
                            onSensorLoaded(sensor);

                            //Load Containervalues if requested
                            loadContainerValues = loadContainerValues || true;
                            if (loadContainerValues == true && $.isFunction(onContainerValueLoaded)) {
                                for (var i = 0; i < sensor.containers.length; i++) {
                                    var container = sensor.containers[i];
                                    container.loadCurrentValue(onContainerValueLoaded);
                                }
                            }
                        }
                    }

                }
            }).fail(function () {
                //If the sensor couldn't be loaded, send back to user
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("Sensor nr." + sensorId + " could not be loaded.");
                }
            });
        }
        onSensorLoaded(null);
    }
    return null;
}
Sensor.loadFromJson = function(json, loadContainerValues, onContainerValueLoaded) {
    if (json != null) {
        var data = JSON.parse(json);

        //Make the sensorobject
        var sensor = new Sensor(data["Id"], data["Name"], data["Company"], data["MACAddress"], data["Location"], data["CreationDate"],
                data["SensorEntries"], data["BasestationLat"], data["BasestationLng"]);

        //Make containerobjects from the data
        var containers = [];
        if (data.hasOwnProperty("Containers") && data["Containers"] != null) {
            for (var i = 0; i < data["Containers"].length; i++) {
                var obj = data["Containers"][i];
                if (obj.hasOwnProperty("Id") && obj.hasOwnProperty("Name")) {
                    //Make containerobject
                    var container = new Container(obj["Id"], obj["Name"], obj["CreationTime"], sensor);

                    //Add to sensor
                    containers.push(container);

                    //Load Containervalues if requested
                    loadContainerValues = loadContainerValues || true;
                    if (loadContainerValues == true && $.isFunction(onContainerValueLoaded)) {
                        container.loadCurrentValue(onContainerValueLoaded);
                    }
                }
            }
        }

        //Make and return sensor from JSON & Containers
        sensor.containers = containers;
        return sensor;
    }

    return null;
};

Group.create = function (name, onGroupSaved) {
    //make a new group object with no sensors
    var group = new Group(null, name, []);

    //Save the object to the database
    group.save(onGroupSaved);

    return group;
}