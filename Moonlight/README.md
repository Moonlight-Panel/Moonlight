### Some explanations

defaultstorage:

This directory is for the default assets of a moonlight instance, e.g. lang files, images etc

storage:

This directory is empty in fresh moonlight instances and will be populated with example config upon first run.
Before using moonlight this config file has to be modified.
Also resources are going to be copied from the default storage to this storage.
To access files in this storage we recommend to use the PathBuilder functions to ensure cross platform compatibility.
The storage directory should be mounted to a specific path when using docker container so when the container is replaced/rebuild your storage will not be modified