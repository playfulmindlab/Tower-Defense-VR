from os import name
from traceback import print_tb
from tracemalloc import stop
from turtle import width
from Phidget22.Phidget import *
from Phidget22.Devices.VoltageRatioInput import *
import time
import numpy as np
from tqdm import tqdm
from matplotlib import pyplot as plt
from pylsl import StreamInfo, StreamOutlet 
import tkinter as tk
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg 
import customtkinter
import keyboard

board_height, board_width = 40, 60 # cms
open_stream = True # keep true if you need LSL
plot = True # keep true to start GUI and plot
test_duration = 10 # seconds

# 3     0
# 2     1

def onVoltageRatioChange(self, voltageRatio):
	if voltageRatio > 0.0001:
	    print("VoltageRatio [" + str(self.getChannel()) + "]: " + str(voltageRatio))

def init_phidgets(serial_num=407303):
    voltageRatioInput0 = VoltageRatioInput()
    voltageRatioInput1 = VoltageRatioInput()
    voltageRatioInput2 = VoltageRatioInput()
    voltageRatioInput3 = VoltageRatioInput()

    voltageRatioInputs = [voltageRatioInput0, voltageRatioInput1, voltageRatioInput2, voltageRatioInput3]

    for i in range(4):
        voltageRatioInputs[i].setDeviceSerialNumber(serial_num)
        voltageRatioInputs[i].setChannel(i)

    # for voltageRatioInput in voltageRatioInputs:
    #     voltageRatioInput.setOnVoltageRatioChangeHandler(onVoltageRatioChange)

    for voltageRatioInput in voltageRatioInputs:
        voltageRatioInput.openWaitForAttachment(5000)

    return voltageRatioInputs

# Perform calibration from control panel to find offset, this can be used to find an offset by keeping the board empty initially
def calibrate_phidgets(voltageRatioInputs):
    print('Calibrating')
    cal0, cal1, cal2, cal3 = [], [], [], []
    for _ in tqdm(range(300)):
        cal0.append(voltageRatioInputs[0].getVoltageRatio())
        cal1.append(voltageRatioInputs[1].getVoltageRatio())
        cal2.append(voltageRatioInputs[2].getVoltageRatio())
        cal3.append(voltageRatioInputs[3].getVoltageRatio())
        time.sleep(0.01)

    return np.array([np.mean(cal0), np.mean(cal1), np.mean(cal2), np.mean(cal3)])


# Calculates the COP based on the 4 voltage values received
def calcCOP(cal_dat):
    value_sum = cal_dat[0]+cal_dat[1]+cal_dat[2]+cal_dat[3]
    cop_x = board_width/2*(cal_dat[0]+cal_dat[1] - (cal_dat[2]+cal_dat[3])) / value_sum
    cop_y = board_height/2*(cal_dat[0]+cal_dat[3] - (cal_dat[1]+cal_dat[2])) / value_sum
    return [cop_x, cop_y], value_sum / 4


# Plots the movement of center of pressure real time
class plotter:
    def __init__(self):
        fig_width = 6
        plt.rcParams["axes.prop_cycle"] = plt.cycler(color=["#4C2A85", "#BE96FF", "#957DAD", "#5E366E", "#A98CCC"])
        plt.ion()
        self.fig = plt.figure(figsize=(fig_width, fig_width*board_height/board_width))
        ax = self.fig.add_axes([0, 0, 1, 1], frameon=False)
        ax.set_xlim(-board_width/2, board_width/2), ax.set_xticks([])
        ax.set_ylim(-board_height/2, board_height/2), ax.set_yticks([])
        self.scatter = ax.scatter(0, 0, s=200, lw=0.5, facecolors='green', cmap='RdYlGn')
        # plt.draw()
    
    def get_fig(self):
        return self.fig

    def plot(self, x, magnitude):
        self.scatter.set_offsets(x)
        self.scatter.set_sizes([max(10, 5000*magnitude)])
        plt.pause(0.02)


# Code to run one measurement of COP
def start_measurement(fixed_duration=False):
    print('Starting new measurement')
    if open_stream:
        outlet.push_sample(["New Measurement"])
    t_end = time.time() + test_duration
    while 1:
        if fixed_duration and time.time() > t_end:
            break
        sensor_values = np.array([voltageRatioInputs[i].getVoltageRatio() for i in range(4)])
        sensor_values = sensor_values - calibration_offsets
        sensor_values = np.array([0 if v < 0 else v*1000 for v in sensor_values])
        cop, magnitude = calcCOP(sensor_values)
        if plot:
            plotter_.plot(cop, magnitude)
        if open_stream:
            outlet.push_sample(["COP Measurement -> X: " + str(cop[0]) + " Y: " + str(cop[1])])
            time.sleep(0.02)   
        # print('{:.3f}, {:.3f}, {:.3f}, {:.3f}'.format(sensor_values[0], sensor_values[1], sensor_values[2], sensor_values[3]))
    print('Measurement Complete')


# Can be used as an interface to start and stop the measurement cycles
class GUI:
    def __init__(self):
        self.window = customtkinter.CTk()
        self.window.title("BTracks Balance Board")
        self.window.geometry(f"{200}x{400}")

        self.window.sidebar_frame = customtkinter.CTkFrame(self.window, width=240)
        self.window.sidebar_frame.grid(row=0, column=0, rowspan=5, sticky="nsew")
        self.window.sidebar_frame.grid_rowconfigure(5, weight=1)
        self.window.logo_label = customtkinter.CTkLabel(self.window.sidebar_frame, text="Experiment Settings", font=customtkinter.CTkFont(size=16, weight="bold"))
        self.window.logo_label.grid(row=0, column=0, columnspan=2, padx=20, pady=(20, 10))

        self.experiment_name = customtkinter.StringVar(value="")
        self.window.entry = customtkinter.CTkEntry(self.window.sidebar_frame, placeholder_text="Experiment Name", textvariable=self.experiment_name)
        self.window.entry.grid(row=1, column=0, columnspan=2, padx=10, pady=(20, 20))

        self.fixed_duration = customtkinter.StringVar(value="True")
        self.window.checkbox_1 = customtkinter.CTkCheckBox(master=self.window.sidebar_frame, text="Fixed Duration", checkbox_width=17, checkbox_height=17, 
                                                        border_width=2, variable=self.fixed_duration, onvalue="True", offvalue="False")
        self.window.checkbox_1.grid(row=2, column=0, columnspan=2,padx=20, pady=10)

        self.duration = customtkinter.StringVar(value="10")
        self.window.entry2 = customtkinter.CTkEntry(self.window.sidebar_frame, width=50, placeholder_text="10", textvariable=self.duration)
        self.window.entry2.grid(row=3, column=1, columnspan=1, padx=2, pady=(2, 2))
        self.window.logo_label = customtkinter.CTkLabel(self.window.sidebar_frame, text="Duration: ")
        self.window.logo_label.grid(row=3, column=0, columnspan=1, padx=20, pady=(20, 10))
        self.window.sidebar_button_4 = customtkinter.CTkButton(self.window.sidebar_frame, command=self.sidebar_button_event, text="Start Measurement")
        self.window.sidebar_button_4.grid(row=4, column=0, columnspan=2, padx=20, pady=(20, 10))


        self.sway = customtkinter.StringVar(value=" ")
        self.window.entry3 = customtkinter.CTkLabel(self.window.sidebar_frame, text="")
        self.window.entry3.grid(row=5, column=1, columnspan=1, padx=2, pady=(2, 2))
        self.window.logo_label2 = customtkinter.CTkLabel(self.window.sidebar_frame, text="Sway Distance: ")
        self.window.logo_label2.grid(row=5, column=0, columnspan=1, padx=20, pady=(20, 10))

        # self.window.main_frame = customtkinter.CTkFrame(self.window, width=800, height=1024)
        # self.window.main_frame.grid(row=0, column=1, rowspan=1, sticky="nsew")
        # self.window.main_frame.grid_rowconfigure(2, weight=1)
        # canvas1 = FigureCanvasTkAgg(fig, self.window.main_frame)
        # canvas1.draw()
        # canvas1.get_tk_widget().pack()
        self.window.mainloop()

    def sidebar_button_event(self):
        print('Starting new measurement. Press space bar to end trial.')
        experiment_name = "Tower Defense"        

        if open_stream:
            outlet.push_sample(["New Trial: " + experiment_name])

        xcoordinates, ycoordinates = [], []
        magnitudes = []
        while 1:
            sensor_values = np.array([voltageRatioInputs[i].getVoltageRatio() for i in range(4)])
            sensor_values = sensor_values - calibration_offsets
            sensor_values = np.array([0 if v < 0 else v*1000 for v in sensor_values])
            cop, magnitude = calcCOP(sensor_values)
            magnitudes.append(magnitude)
            if magnitude > 0.008:
                xcoordinates.append(cop[0])
                ycoordinates.append(cop[1])
            if open_stream:
                outlet.push_sample(["COP Measurement -> X: {:.3f} Y: {:.3f} Mag: {:.4f}".format(cop[0], cop[1], magnitude)])
            if not plot:
                time.sleep(0.02)
            if keyboard.is_pressed(" "):
                break
                
        print('Measurement Complete')
        if self.fixed_duration.get() == "True":
            x = np.array(xcoordinates)
            y = np.array(ycoordinates)
            dist_array = (x[:-1]-x[1:])**2 + (y[:-1]-y[1:])**2
            sway = np.sum(np.sqrt(dist_array)) 
            self.window.entry3.configure(text="{:.2f}".format(sway))
            outlet.push_sample(["Trial complete with Sway: " + str(sway)])



def perform_streaming():
    print('Starting stream. Press space bar to end.')

    experiment_name = "Tower Defense"        

    if open_stream:
        outlet.push_sample(["New Trial: " + experiment_name])

    xcoordinates, ycoordinates = [], []
    magnitudes = []
    while 1:
        sensor_values = np.array([voltageRatioInputs[i].getVoltageRatio() for i in range(4)])
        sensor_values = sensor_values - calibration_offsets
        sensor_values = np.array([0 if v < 0 else v*1000 for v in sensor_values])
        cop, magnitude = calcCOP(sensor_values)
        magnitudes.append(magnitude)
        if magnitude > 0.008:
            xcoordinates.append(cop[0])
            ycoordinates.append(cop[1])
        if open_stream:
            outlet.push_sample(["COP Measurement -> X: {:.3f} Y: {:.3f} Mag: {:.4f}".format(cop[0], cop[1], magnitude)])
        if not plot:
            time.sleep(0.02)
        if keyboard.is_pressed(" "):
            break
                
    print('Measurement Complete')
    x = np.array(xcoordinates)
    y = np.array(ycoordinates)
    dist_array = (x[:-1]-x[1:])**2 + (y[:-1]-y[1:])**2
    sway = np.sum(np.sqrt(dist_array)) 
    outlet.push_sample(["Trial complete with Sway: " + str(sway)])
    print("Measured sway is: "+str(sway))




if __name__ == '__main__':
    try:
        voltageRatioInputs = init_phidgets()
        calibration_offsets = calibrate_phidgets(voltageRatioInputs)
        if open_stream:
            info = StreamInfo('BalanceBoard', 'EEG', channel_count=1, channel_format='string')
            outlet = StreamOutlet(info)
            perform_streaming()

    except KeyboardInterrupt:
        pass

    for voltageRatioInput in voltageRatioInputs:
        voltageRatioInput.close()
