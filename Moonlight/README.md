# Some explanations
defaultstorage:

This directory is for the default assets of a Moonlight instance, e.g., Lang files, images, etc.

Storage:

This directory is empty in fresh Moonlight instances and will be populated with an example configuration upon first run. Before using Moonlight, this config file has to be modified. Also, resources are going to be copied from the default storage to this storage. To access files in this storage, we recommend using the Path Builder functions to ensure cross-platform compatibility. The storage directory should be mounted to a specific path when using a Docker container, so when the container is replaced or rebuilt,  your storage will not be modified.
