import numpy as np
import sys

def main(filename1, filename2, filename3):
    arr = np.load(filename1).reshape((30,1))
    print(arr)

if __name__ == "__main__":
    main(sys.argv[1])