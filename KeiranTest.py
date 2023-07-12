#! /user/bin/python

import math
import time
import six
from pyquaternion import Quaternion
from sksurgerynditracker.nditracker import NDITracker
import os

settings_aurora = {
    "tracker type": "aurora",
    "ports to probe": 2,
    "verbose": True,
    "use quaternions": True,
}

tracker = NDITracker(settings_aurora)
tracker.start_tracking()

try:
    while True:
        port_handles, timestamps, frame_numbers, tracking_data, quality = tracker.get_frame()
        """print(t)
        print(port_handles[t])"""
        array0 = tracking_data[0]
        array1 = tracking_data[1]
        ind_pos = [4,5,6]
        distance = math.dist(array1[0,ind_pos],array0[0,ind_pos])
        #print("##########################\n")
        print("Percent Error:"+str((abs(12-distance)/12)*100))
        print("mm Error:"+str(12-distance))
        print("Quality:"+str(quality))
        print("----------------------------\n")
        os.system('cls')
        time.sleep(0.025)
except KeyboardInterrupt:
    pass
tracker.stop_tracking()
tracker.close()


