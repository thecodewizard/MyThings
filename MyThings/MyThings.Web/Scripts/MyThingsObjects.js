//This is the javascript file with the objects
    //Always load this AFTER jquery and BEFORE other self made scripts

//Serverlocations
//var apiBaseUrl = "http://localhost:22056/api/";
var apiBaseUrl = "http://mythingsapi.azurewebsites.net/api/";
var siteBaseUrl = "http://localhost:16964/Dashboard/";

//OBJECTS
function Sensor(id, name, company, macaddress, location, creationdate, sensorentries, basestationlat, basestationlng, containers) {
    //Fields
    this.id = id;
    this.name = name;
    this.company = company;
    this.macaddress = macaddress;
    this.location = location;
    this.creationDate = creationdate;
    this.sensorEntries = sensorentries;
    this.basestationLat = basestationlat;
    this.basestationLng = basestationlng;
    this.containers = containers;

    //Make an internally callable object
    var that = this;

    //Functions //TODO: Update API URL's
    this.update = function(onSensorUpdated) {
        //send to url in api/get/getSensor/id
        //onSensorUpdated is a function which expects a sensorobject as argument.

        if (that.id != null && that.name != null) {
            $.ajax({
                    url: apiBaseUrl + "get/getSensor?sensorId=" + that.sensor.id,
                    method: "GET"
            }).done(function(json) {
                if (json != null) {
                    var dataUpdate = JSON.parse(json);
                    that.containers = dataUpdate.Containers;
                    that.basestationLat = dataUpdate.BasestationLat;
                    that.basestationLng = dataUpdate.BasestationLng;
                    that.company = dataUpdate.Company;
                    that.location = dataUpdate.Location;
                    that.name = dataUpdate.Name;
                    that.sensorEntries = dataUpdate.SensorEntries;
                    if ($.isFunction(onSensorUpdated)) {
                        onSensorUpdated(that);
                    }
                }
            }).fail(function(json) {
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("The sensor" +
                        that.sensor.id +
                        " could not be loaded.");
                }
            });
        }



    };
    this.pin = function(onSensorPinned) {
        //send to url in api/post/pinSensor/id
        //onSensorPinned is a function which expects a sensorobject as argument

        if (that.id != null && that.name != null) {
            $.ajax({
                url: apiBaseUrl + "post/pinSensor?sensorId=" + that.sensor.id,
                method: "POST"
            }).done(function (json) {
                if (json != null) {
                    var dataUpdate = JSON.parse(json);
                    
                    that.id = dataUpdate.Id;
                    if ($.isFunction(onSensorPinned)) {
                        onSensorPinned(that);
                    }
                }
            }).fail(function (json) {
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("The sensor" +
                        that.sensor.id +
                        " could not be pinned.");
                }
            }).statusCode(function (json) {
                if (json != null) {
                   // json.statusCode.
                }
            });
        }


    };
}

function Container(id, name, macaddress, creationtime, lastupdatedtime, containertype, sensorId, currentValue, history) {
    //Fields
    this.id = id;
    this.name = name;
    this.macaddress = macaddress;
    this.creationTime = creationtime;
    this.lastUpdatedTime = lastupdatedtime;

    this.containerType = containertype;
    this.sensorId = sensorId;

    this.currentValue = currentValue;
    this.history = history;

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
                })
                .fail(function() {
                    //If the value couldn't be loaded, send back to user
                    if ($.isFunction(MyThings.logToUser)) {
                        MyThings.logToUser("The value of container nr." +
                            that.id +
                            " (" +
                            that.name +
                            ") could not be loaded.");
                    }
                });
        }
    };
    this.update = function(onContainerUpdated) {
        //send to url in api/get/getContainer/id
        //onContainerUpdated is a function which expects a containerobject as argument.

        if (that.id != null && that.name != null) {
            $.ajax({
                url: apiBaseUrl + "get/getContainer?containerId=" + that.container.id,
                method: "GET"
            }).done(function (json) {
                if (json != null) {
                    var dataUpdate = JSON.parse(json);
                    that.name = dataUpdate.Name;
                    that.history = dataUpdate.History;
                    if ($.isFunction(onContainerUpdated)) {
                        onContainerUpdated(that);
                    }
                }
            }).fail(function (json) {
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("The container" +
                        that.container.id +
                        " could not be loaded.");
                }
            });
        }


        if ($.isFunction(onContainerUpdated)) {
            onContainerUpdated(that);
        }
    };
    this.pin = function(onContainerPinned) {
        //send to url in api/post/pinContainer/id
        //onContainerPinned is a function which expects a containerobject as argument

        if (that.id != null && that.name != null) {
            $.ajax({
                url: apiBaseUrl + "post/pinContainer?containerId=" + that.container.id,
                method: "POST"
            }).done(function (json) {
                if (json != null) {
                    var dataUpdate = JSON.parse(json);
                    that.id = dataUpdate.Id;
                    if ($.isFunction(onContainerPinned)) {
                        onContainerPinned(that);
                    }
                }
            }).fail(function (json) {
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("The container" +
                        that.container.id +
                        " could not be pinned.");
                }
            });
        }

    };
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
    };
    this.update = function(onGroupUpdated) {
        //send to url in api/get/getGroup/id
        //onGroupUpdated is a function which expects a groupobject as argument

        if (that.id != null && that.name != null) {
            $.ajax({
                url: apiBaseUrl + "get/getGroup?groupId=" + that.group.id,
                method: "GET"
            }).done(function (json) {
                if (json != null) {
                    var dataUpdate = JSON.parse(json);
                    that.name = dataUpdate.Name;
                    that.sensors = dataUpdate.Sensors;
                    if ($.isFunction(onGroupUpdated)) {
                        onGroupUpdated(that);
                    }
                }
            }).fail(function (json) {
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("The group" +
                        that.group.id +
                        " could not be loaded.");
                }
            });
        }
    };
    this.pin = function(onGroupPinned) {
        //send to url in api/post/pinGroup/id
        //onGroupPinned is a function which expects a groupobject as argument

        if (that.id != null && that.name != null) {
            $.ajax({
                url: apiBaseUrl + "post/pinGroup?groupId=" + that.group.id,
                method: "POST"
            }).done(function (json) {
                if (json != null) {
                    var dataUpdate = JSON.parse(json);
                    that.id = dataUpdate.Id;
                    if ($.isFunction(onGroupPinned)) {
                        onGroupPinned(that);
                    }
                }
            }).fail(function (json) {
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("The group" +
                        that.group.id +
                        " could not be pinned.");
                }
            });
        }

    };
    this.addSensor = function(sensor, onGroupSaved) {
        //send to url in api/post/addToGroup/sensor

        //Send object changes to database
        that.save(onGroupSaved);
    };
    this.removeSensor = function(sensor, onGroupSaved) {
        //send to url in api/post/removeFromGroup/sensor

        //Send object changes to database
        that.save(onGroupSaved);
    };
    this.hasSensor = function(sensor) {
        //check in the that.sensors whether the given sensor is found.
        //return boolean
    };
}

function Error(id, errorcode, type, category, title, description, advice, time, read, sensor, container) {
    this.id = id;
    this.errorCode = errorcode;
    this.type = type;
    this.category = category;
    this.title = title;
    this.description = description;
    this.advice = advice;
    this.time = time;
    this.read = read;
    this.sensor = sensor;
    this.container = container;

    //Make an internally callable object
    var that = this;

    this.update = function(onErrorUpdated) {
        //send to url in api/get/getError/id
        //onErrorUpdated is a function which expects a errorobject as argument

        if (that.id != null && that.name != null) {
            $.ajax({
                url: apiBaseUrl + "get/getError?errorId=" + that.error.id,
                method: "GET"
            }).done(function (json) {
                if (json != null) {
                    var dataUpdate = JSON.parse(json);
                    that.description = dataUpdate.Description;
                    that.advice = dataUpdate.Advice;
                    if ($.isFunction(onErrorUpdated)) {
                        onErrorUpdated(that);
                    }
                }
            }).fail(function (json) {
                if ($.isFunction(MyThings.logToUser)) {
                    MyThings.logToUser("The error" +
                        that.error.id +
                        " could not be loaded.");
                }
            });
        }

    };
    this.pin = function(onErrorPinned) {
        //send to url in api/post/pinError/id
        //onErrorPinned is a function which expects a errorObject as argument

        if ($.isFunction(onErrorPinned)) {
            onErrorPinned(that);
        }
    };
}

function ContainerType(id, name) {
    //Fields
    this.id = id;
    this.name = name;
}

function ContainerValue(value, timestamp) {
    //Fields
    this.value = value;
    this.timestamp = timestamp;
}

function Tile(id, col, row, sizeX, sizeY, pin) {
    this.id = id;
    this.col = col;
    this.row = row;
    this.sizeX = sizeX;
    this.sizeY = sizeY;
    this.pin = pin;
}

function Pin(id, userid, tileid, savedId, savedType, isDeleted) {
    this.id = id;
    this.userId = userid;
    this.tileId = tileid;
    this.savedId = savedId;
    this.savedType = savedType;
    this.isDeleted = isDeleted;
}

//STATIC CREATORS
Container.loadFromJson = function (json) {
    if (json != null) {
        var data = JSON.parse(json);

        var containers = [];
        if (data.hasOwnProperty("Containers") && data["Containers"] != null) {
            for (var i = 0; i < data["Containers"].length; i++) {
                var obj = data["Containers"][i];
                if (obj.hasOwnProperty("Id") && obj.hasOwnProperty("Name")) {
                    //Make containerobject
                    var container = new Container(data["id"], data["name"], data["macaddress"], data["creationtime"], data["lastupdatedtime"], data["containertype"],
                                    data["sensorId"], data["currentValue"], data["history"]);

                    //Add to containers
                    containers.push(container);
                }
            }
        }

        return containers;

    }
    return null;
}

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
Sensor.loadMany = function(count, onSensorLoaded, loadContainerValues, onContainerValueLoaded) {
    if ($.isFunction(onSensorLoaded)) {
        if (count != null) {
            $.ajax({
                    url: apiBaseUrl + "get/getsensors?count=" + count,
                    method: "GET",
                    contentType: "application/json"
                })
                .done(function(json) {
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
                })
                .fail(function() {
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

Group.create = function(name, onGroupSaved) {
    //make a new group object with no sensors
    var group = new Group(null, name, []);

    //Save the object to the database
    group.save(onGroupSaved);

    return group;
};