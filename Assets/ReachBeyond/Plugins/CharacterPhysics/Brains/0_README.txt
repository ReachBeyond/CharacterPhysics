These are the things that "drive" character movement. These decouple the
movement from inputs and/or logic that drive them.

In general, there would be a separate driver for each player type (if we
do many) and separate driver for each enemy type. However, they generally yield
up the same delegates.
