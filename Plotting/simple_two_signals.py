import matplotlib.pyplot as plt
import numpy as np

def main():
   # Printing two simple signals (the one with double the frequency of the other) with their phases.

   # Plotting signal1
   x1 = np.linspace(np.pi/2, 5/2 * np.pi, 200)
   signal1 = 1*np.sin(1*x1)
   plt.subplot(2,2,1)
   plt.title("a '1 Hz sinusoidal'")
   plt.xticks(np.linspace(np.pi/2, 5/2 * np.pi, 5), ["0", "1/2 π", " π", "3/2 π" ,"2π"]) # np.linspace(np.pi/2, 5/2 *np.pi, 9), ["1/2 π", "3/4 π", "π", "5/4 π", "3/2 π", "7/4 π", "2π", "9/4 π", "5/2 π"]
   plt.yticks(np.arange(-1, 2, 1))
   plt.plot(x1, signal1)
   
   # Plotting phase of signal1
   xp1 = np.linspace(0, 1, 100)
   phase1 = 1*xp1
   plt.subplot(2,2,2)
   plt.title("Phase of a '1 Hz sinusoidal'")
   plt.xticks([0, 1/4, 1/2, 3/4, 1], ["0", "1/2 π", " π", "3/2 π" ,"2π"])
   plt.yticks(np.arange(0, 1.25, 0.25))
   plt.plot(xp1, phase1)
   
   
   # Plotting signal2
   x2 = np.linspace(np.pi/4, 9/4 * np.pi, 200)
   signal2 = 1*np.sin(2*x2)
   plt.subplot(2,2,3)
   plt.title("a '2 Hz sinusoidal'")
   plt.xticks(np.linspace(np.pi/4, 9/4 * np.pi, 5), ["0", "π", "2π", "3π", "4π"]) # np.linspace(np.pi/4, 9/4 *np.pi, 9), ["1/4 π", "1/2 π", "3/4 π", "π", "5/4 π", "3/2 π", "7/4 π", "2π", "9/4 π"]
   plt.yticks(np.arange(-1, 2, 1))
   plt.plot(x2, signal2)
   
   # Plotting phase of signal2
   xp2 = np.linspace(0, 1.999999, 100)
   phase2 = 1*xp2 % 1
   np.append(phase2, 1)
   plt.subplot(2,2,4)
   plt.title("Phase of a '2 Hz sinusoidal'")
   plt.xticks([0, 1/2, 1, 3/2, 2], ["0", "π", "2π", "3π", "4π"])
   plt.yticks(np.arange(0, 1.25, 0.25))
   plt.plot(xp2, phase2)
   
   
   # Plotting the whole resulting Figure
   plt.tight_layout()
   plt.suptitle("Two simple signals and their phases", fontweight='bold')
   plt.subplots_adjust(top=0.85)
   plt.show()
   
    
if __name__ == "__main__":
    main()
