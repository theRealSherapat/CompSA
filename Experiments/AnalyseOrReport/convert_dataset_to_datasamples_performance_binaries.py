import sys
import matplotlib.pyplot as plt
import numpy as np
import csv

def main(CSV_filepath, plotFailuresPls):
    matrixOfCSVValues, csvHeader = parseDataFrom(CSV_filepath)
    
    sampleStartIndexes, csvHeaderPrefixIndex = getSampleStartIndexes(matrixOfCSVValues)    
    csvHeaderPrefix = csvHeader[csvHeaderPrefixIndex]
    
    dataSamples = getSplitUpSamples(matrixOfCSVValues, sampleStartIndexes) # a Python list    
    
    saveBinariesForEachSample(dataSamples, csvHeader, plotFailuresPls)
    
    
    

def saveBinariesForEachSample(dataSamples, binaryPrefix, plotFailuresPls):
    for sampleInd, dataSample in enumerate(dataSamples):
        terminationTimePerfScores = dataSample[:,0]
        simSuccessesPerfScores = dataSample[:,1]
        
        if plotFailuresPls == 0:
            terminationTimePerfScores = terminationTimePerfScores[simSuccessesPerfScores == 1]
        
        fullBinaryFilename = "./ConvertedBinaries/dataSampleBinary_"
        
        np.save(fullBinaryFilename + "terminationTimes_" + str(sampleInd), terminationTimePerfScores)
        np.save(fullBinaryFilename + "successes_" + str(sampleInd), simSuccessesPerfScores)


def getSplitUpSamples(matrixOfCSVValues, sampleStartIndexes):
    dataSamples = []
    
    for i in range(1,len(sampleStartIndexes)):
        rowStartIndex = sampleStartIndexes[i-1]
        rowEndIndex = sampleStartIndexes[i]
        dataSamples.append(matrixOfCSVValues[rowStartIndex:rowEndIndex,:])

    rowStartIndex = sampleStartIndexes[-1]
    dataSamples.append(matrixOfCSVValues[rowStartIndex:,:])
    
    return dataSamples

def getSampleStartIndexes(hugi_datamatrix):
    """ Return the (30 typically) rows of the hugi datamatrix that belong together (in a nested Python list with np.ndarrays in it for each data sample). """
    
    headerIndex = 0
    newDatasampleIndexes = []
    
    hugi_covariate_datamatrix = hugi_datamatrix[:,2:-1]
    
    previous_covariate_row = np.zeros(hugi_covariate_datamatrix.shape[1])
    for rowInd, row in enumerate(hugi_covariate_datamatrix):
        if not (previous_covariate_row == row).all():
            newDatasampleIndexes.append(rowInd)
            headerIndex = list((previous_covariate_row == row)).index(False)
            
        previous_covariate_row = row
    
    return newDatasampleIndexes, headerIndex


def parseDataFrom(csv_filename): # GOLDEN STANDARD
    """ Reads all rows (apart from the header) into a numpy data-matrix, and returns that 'arrayOfDatapoints' """

    arrayOfDatapoints = np.empty((1, 1)) # initializing our numpy data-matrix with a dummy size (real size will be sat later on)
    
    csvHeader = []
    
    numOfRows = 0
    with open(csv_filename) as dataFile:
        csvReader = csv.reader(dataFile, delimiter=';')
        csvHeader = next(csvReader, None) # to skip the headers
        csvHeader = csvHeader[2:] # only want covariates, so we slice out the measurement header entries.
        for row in csvReader:
            if numOfRows == 0:
                arrayOfDatapoints = np.empty((1, len(row))) # initializing our numpy data-matrix when we know the number of columns to include in it
                
                
        
            col_array = np.array([])
            for col_index in range(len(row)):
                col_element_string = row[col_index].replace(',','.')
                col_element = float(col_element_string)
                col_array = np.append(col_array, col_element)
            
            col_array = np.reshape(col_array, (1, len(row)))
            arrayOfDatapoints = np.vstack((arrayOfDatapoints, col_array))
            
            numOfRows += 1
    
    return arrayOfDatapoints[1:,:], csvHeader # slicer fra og med den 1nte indeksen siden første rad er initialized to 0 by using np.empty()

if __name__ == "__main__":
    # Args: 1) dataset.csv filepath, optional 2) (int) whether we want to plot failures or not (0: no, 1: yes)
    
    # Takes as input and extracts (both success and failure) synchronization times from a .CSV-file representing a dataset from several runs of the Harmonic Synchrony Simulation with the Dr. Squiggles Musical Multi-Robot System.
   
    # The reason for taking in the plotFailuresPls argument here is since it's most practical (even though it's not the most professional).
    
    filepath = sys.argv[1]
    
    # Optional keeping of failed synchronization runs:
    try:
        integerInputDuima = int(sys.argv[-1])
        plotFailuresPls = integerInputDuima
    except ValueError:
        plotFailuresPls = 0
    
    main(filepath, plotFailuresPls)