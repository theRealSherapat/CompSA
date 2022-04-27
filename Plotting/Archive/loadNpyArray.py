import numpy as np
import sys

def main(filename):
    arr = np.load(filename)

if __name__ == "__main__":
    main(sys.argv[1])