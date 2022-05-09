import os
import sys
import csv
import multiprocessing as mp

sampSize = 30
reservedProcessors = 2

# Default values unless optional hyperparameters are given per simulation run:
defaultHyperparamsDictionary = {'COLLSIZE'                              : 3,
                                'NYMOENREC'                             : 0,
                                'TREFPERC'                              : 0.05,
                                'MINFREQ'                               : 0.5,  # Hz
                                'MAXFREQ'                               : 4,    # Hz
                                'K'                                     : 8,
                                'T_F'                                   : 0.08, # s
                                'TQDEFINER'                             : 0,
                                'USE_DETERMINISTIC_SEED'                : 0,
                                'RANDOMSEED'                            : 0,
                                'SIMMODE'                               : 0,
                                'SAMPLERATE'                            : 50,   # Hz
                                'MAXTIMELIMIT'                          : 300,  # s
                                'ALLOW_ROBOTS_TO_STRUGGLE_FOR_PERIODS'  : 3,    # oscillator periods (given fundamental / lowest frequency)
                                'ALPHA'                                 : 0.8,
                                'PHASEADJ'                              : 0,
                                'BETA'                                  : 0.4,
                                'FREQADJ'                               : 0,
                                'M'                                     : 5
                                } # the values of which have been found decent through empirical experience.



# Assigning of hyperparameters before a batch of simulation runs:

def assignNonDefaultHyperparameters(**kwargs):
    resultHyperparamsDict = konstruerRiktigHyperparamsDictionary(kwargs)
    writeHyperparametersToCSV(resultHyperparamsDict)

def konstruerRiktigHyperparamsDictionary(nonDefaultKWArgs):
    """ Constructs and returns a Python dictionary with all the default hyperparameters overrided by the non-default hyperparameters if they are given. """
    hyperparamsDictionary = defaultHyperparamsDictionary
        
    for nonDefaultArgKey in nonDefaultKWArgs.keys():
        uppercaseNonDefaultArgKey = str(nonDefaultArgKey).upper()
        if not (uppercaseNonDefaultArgKey in defaultHyperparamsDictionary):
            print("Bruh, jeg har da aldri hørt om noe slags hyperparameter kalt '" + uppercaseNonDefaultArgKey + "'... Jeg returner None her jeg..")
            return None
        hyperparamsDictionary[uppercaseNonDefaultArgKey]=nonDefaultKWArgs[str(nonDefaultArgKey)]
    
    return hyperparamsDictionary

def writeHyperparametersToCSV(hyperparameterDict):
    """ Writes the resulting wanted hyperparameters (contained in the Python dictionary) to a hyperparameter .CSV file which the Unity synchrony simulator loads in and assigns to its internal hyperparameters before running a simulation run. """
    
    # Writing the .CSV header containing covariate / hyperparameter names, followed by corresponding wanted covariates / hyperparameters in the next row in a .CSV file:
    
    hyperparameterHeaderList = list(hyperparameterDict.keys())
    hyperparameterCovariatesList = byttUtPunktumerMedKommaer(list(hyperparameterDict.values()))
    
    with open('wantedHyperparametersForSimulationRun.csv', 'w', newline='') as csvfile:
        spamwriter = csv.writer(csvfile, delimiter=';') # Kan bruke de siste to parameterne: quotechar='|', quoting=csv.QUOTE_MINIMAL
        spamwriter.writerow(hyperparameterHeaderList)
        spamwriter.writerow(hyperparameterCovariatesList)

def byttUtPunktumerMedKommaer(listoMedNumeriskeVerdior):
    stringListWithNoPeriodsOnlyCommas = []
    
    for verdi in listoMedNumeriskeVerdior:
        stringListWithNoPeriodsOnlyCommas.append(str(verdi).replace(".",","))
    
    return stringListWithNoPeriodsOnlyCommas



# Running Unity Synchrony simulator runs:

def runNumberOfSimulationRuns(no_of_runs):    
    pool = mp.Pool(mp.cpu_count()-reservedProcessors)
    
    pool.map(run_synchrony_simulator_executable, [i for i in range(no_of_runs)])
    
    pool.close()

def run_synchrony_simulator_executable(iteration_index):
    os.system("SynchronySimulatorRun.exe -batchmode -nographics")