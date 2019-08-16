# -*- coding: utf-8 -*-
"""
Created on Tue Jun 12 10:42:41 2018

@author: binbi
"""


#
import socket
import struct
import numpy as np

import matplotlib.pyplot as plt
import matplotlib.animation as animation


fs = 2000; # Myon Sample time 2000hz 

# Set for how long the live processing should last (in seconds)
endTime = 10; # Set the time of acquistion here
frameCount = 0;
channel = 6
RawEMG = np.zeros((fs*endTime,channel)); 
EMGwindow = 0;
n = 480; #Buffer size
p = 20; # Buffer overlap
k = 0; #Used to save the RawEMG data



def moveMea(mylist,N):

    cumsum, moving_aves = [0], []
    
    for i, x in enumerate(mylist, 1):
        cumsum.append(cumsum[i-1] + x)
        if i>=N:
            moving_ave = (cumsum[i] - cumsum[i-N])/N
            #can do stuff with moving_ave here
            moving_aves.append(moving_ave)
            
    return moving_aves




TCP_IP = '127.0.0.1'
TCP_PORT = 5000
BUFFER_SIZE = 1024


s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((TCP_IP, TCP_PORT))





# Parameters
x_len = 2000         # Number of points to display
y_range = [-1000, 1000]  # Range of possible Y values to display
dataRev = np.ones([1000000,2])

# Create figure for plotting
fig = plt.figure()
plt.subplots_adjust(hspace=0.5)
ax = fig.add_subplot(2, 1, 1)
ax1 = fig.add_subplot(2,1,2)

xs = list(range(0, 2000))
ys = [0] * x_len
ys1= [0] * x_len
ax.set_ylim(y_range)
ax1.set_ylim(y_range)

# Create a blank line. We will update the line in animate
line,= ax.plot(xs, ys)
line1,= ax1.plot(xs,ys1)


#ax.title('IMU angular velocity')
#ax.yaxis('angular velocity D/S')
#ax1.yaxis('angular velocity D/S')
# Add labels

plt.xlabel('Samples')


# This function is called periodically from FuncAnimation
def animate(i,ys,ys1):

    # Read temperature (Celsius) from TMP102

    dataRecv = s.recv(8)
    data = struct.unpack('2f',dataRecv)
    data = np.ravel(data)
    dataRev[i] = data
    # Add y to list
    ys.append(dataRev[i,0])
    ys1.append(dataRev[i,1])
    

    # Limit y list to set number of items
    ys = ys[-x_len:]
    ys1 = ys1[-x_len:]

    # Update line with new Y values
    line.set_ydata(ys)
    line1.set_ydata(ys1)
    #line.set_ydata(np.sin(xs + i / 100))
    print ("i=",i)
    print("dataRev0 =",dataRev[i,0])
    print("dataRev1 =",dataRev[i,1])

    return line, line1

# Set up plot to call animate() function periodically
ani = animation.FuncAnimation(fig,animate,fargs=(ys,ys1,),interval=2,blit=True)
#ani = animation.FuncAnimation(fig,animate,interval=50,blit=True)
plt.show()

