import sys
import matplotlib.pyplot as plt
import numpy as np
import csv

def main(CSV_filepath, binaryFilename):
    matrixOfCSVValues = parseDataFrom(CSV_filepath)
    HSYNCHTIMES = matrixOfCSVValues[:,0] # Slicing the first column only
    
    fullBinaryFilename = "./" + binaryFilename + ".npy"
    np.save(fullBinaryFilename, HSYNCHTIMES)
    
    
def parseDataFrom(csv_filename):
    """ Reads all rows (apart from the header) into a numpy data-matrix, and returns that 'arrayOfDatapoints' """

    arrayOfDatapoints = np.empty((1, 1)) # initializing our numpy data-matrix with a dummy size (real size will be sat later on)
    
    numOfRows = 0
    with open(csv_filename) as dataFile:
        csvReader = csv.reader(dataFile, delimiter=';')
        next(csvReader, None) # to skip the headers
        for row in csvReader:
            if numOfRows == 0:
                arrayOfDatapoints = np.empty((1, len(row))) # initializing our numpy data-matrix when we know the number of columns to include in it
        
            col_array = np.array([])
            for col_index in range(len(row)):
                col_element_string = row[col_index].replace(',','.')
                print("col_element_string:", col_element_string)
                col_element = float(col_element_string)
                col_array = np.append(col_array, col_element)
            
            col_array = np.reshape(col_array, (1, len(row)))
            arrayOfDatapoints = np.vstack((arrayOfDatapoints, col_array))
            
            numOfRows += 1
    
    return arrayOfDatapoints

if __name__ == "__main__":
    # Args: 1) dataset.csv filepath, 2) wanted binary.npy filename
    
    # Takes as input and extracts (both success and failure) synchronization times from a .CSV-file representing a dataset from several runs of the Harmonic Synchrony Simulation with the Dr. Squiggles Musical Multi-Robot System.
    
    # Saves .npy-binaries containing all the relevant performance-scores (harmonic synchronization times in simulation time (s)) from a given set of Simulation-runs with certain covariates (have to be kept control of manually).
    
    filepath = sys.argv[1]
    npyBinaryFilename = sys.argv[2]
    
    main(filepath, npyBinaryFilename)