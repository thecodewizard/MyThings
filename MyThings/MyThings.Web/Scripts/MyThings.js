//This is the general Javascript file
    //The general namespace to avoid conflicts
var MyThings = MyThings || {};
var Test = Test || {}; //TODO: Remove test code

MyThings = {
    //TODO: Add Main Javascript functions here

    logToUser: function(logtext) {
        console.log(logtext);
    }
};

Test = {
    getSensor: function(id) {
        Sensor.load(id,
            function(sensor) {
                console.log(sensor);
            });
    },

    getSensors: function() {
        Sensor.loadMany(50,
            function(sensor) {
                console.log(sensor);
            });
    }
}

//OBJECTS API - MANUAL //TODO: Remove these comment lines.

//This block of comment will explain how to work with the MyThings Objects API. 
//This is also found on GitHub

//These are the objects available in the API:
/* Sensor
*  Container
*  Group
*/

//These objects have the following shared functionality:
/* 1. object.update(onObjectUpdated);
        -> Update object values (by id) from server. Triggers onObjectUpdated(object) when ready
   
   2. object.pin(onObjectPinned);
        -> Pin the object. Triggers onObjectPinned(object) when ready.
*/

//The Sensor object has the following extra (not shared) functionality:
/* 1. var sensor = Sensor.load(sensorId, onSensorLoaded, loadContainerValues, onContainerValueLoaded);
        -> Loads sensor by id from server. Triggers the onSensorLoaded(sensor) when successfully loaded.
        -> if loadContainerValues is 'true', the onContainerValueLoaded(container) triggers per successfully loaded container.

   2. var sensor = Sensor.loadFromJson(json, loadContainerValues, onContainerValueLoaded);
        -> Parses sensor from json. loadContainervalues fetches current values from database and triggers onContainerValueLoaded(container) per loaded container.
*/

//The Container object has the following extra (not shared) functionality:
/* 1. container.loadCurrentValue(onCurrentValueLoaded);
        -> loads the most recent value for the container. Triggers onCurrentValueLoaded(container) when successfully loaded.
*/

//The Group object has the following extra (not shared) functionality:
/* 1. var group = Group.create(groupName, onGroupSaved); 
        -> Creates a group clientside and returns it immediately. Sends it to the database and triggers onGroupSaved(group) when completed.
   
   2. group.addSensor(sensor, onGroupSaved);
        -> Add a sensor to the group instantly. Triggers onGroupSaved(group) when the change is pushed to the database.
   
   3. group.removeSensor(sensor, onGroupSaved); //Remove a sensor from the group
        -> Removes a sensor to the group instantly. Triggers onGroupSaved(group) when the change is pushed to the database.
   
   4. group.hasSensor(sensor); //Checks if a sensor is part of the group. (client side)

   5. group.save(onGroupSaved); //Saves a group to the database. Triggers onGroupSaved(group) when successful.
*/