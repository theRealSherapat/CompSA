import numpy as np
import sys

def main(filename1):
    arr = np.load(filename1)
    print(arr.shape)

if __name__ == "__main__":
    main(sys.argv[1])