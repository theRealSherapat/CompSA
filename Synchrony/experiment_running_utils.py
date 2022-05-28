import os
import sys
import csv
import numpy as np
import multiprocessing as mp

reservedProcessors = 0
silent = True

sampSize = 30

# Default values unless optional hyperparameters are given per simulation run:
defaultHyperparamsDictionary = {'COLLSIZE'                              : 3,
                                'NYMOENREC'                             : 0,
                                'TREFDYN'                               : 0.1,
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
                                # To-be-listified (as they are for each chronological agentID):
                                'ALPHAS'                                : 0.8,
                                'ADJ_PHIS'                              : 0,
                                'BETAS'                                 : 0.4,
                                'ADJ_OMEGAS'                            : 0,
                                'MS'                                    : 5,
                                'K_SS'                                  : 100000,
                                'D_SS'                                  : 0
                                } # the values of which have been found decent through empirical experience.

def listifyChronologicalAndDefaultAgentIDHyperparameters():
    defaultHyperparamsDictionary['ALPHAS'] = [defaultHyperparamsDictionary['ALPHAS']]*defaultHyperparamsDictionary['COLLSIZE']
    defaultHyperparamsDictionary['ADJ_PHIS'] = [defaultHyperparamsDictionary['ADJ_PHIS']]*defaultHyperparamsDictionary['COLLSIZE']
    defaultHyperparamsDictionary['BETAS'] = [defaultHyperparamsDictionary['BETAS']]*defaultHyperparamsDictionary['COLLSIZE']
    defaultHyperparamsDictionary['ADJ_OMEGAS'] = [defaultHyperparamsDictionary['ADJ_OMEGAS']]*defaultHyperparamsDictionary['COLLSIZE']
    defaultHyperparamsDictionary['MS'] = [defaultHyperparamsDictionary['MS']]*defaultHyperparamsDictionary['COLLSIZE']
    defaultHyperparamsDictionary['K_SS'] = [defaultHyperparamsDictionary['K_SS']]*defaultHyperparamsDictionary['COLLSIZE']
    defaultHyperparamsDictionary['D_SS'] = [defaultHyperparamsDictionary['D_SS']]*defaultHyperparamsDictionary['COLLSIZE']

listifyChronologicalAndDefaultAgentIDHyperparameters()


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
    
    hyperparamsDictionary = listifySingletonAgentIDCovariates(hyperparamsDictionary, hyperparamsDictionary['COLLSIZE'])
    
    return hyperparamsDictionary

def listifySingletonAgentIDCovariates(dic, R_size):
    """ Turning singleton float or int individual agent hyperparameters into lists of length |R| lists of said hyperparameter value (for each agentID), and lists that are not the size of the number of agents we have (as per default) made into their first value times |R|. """

    if type(dic['ALPHAS']) != list:
        dic['ALPHAS'] = [dic['ALPHAS']]*R_size
    elif len(dic['ALPHAS']) != R_size:
        dic['ALPHAS'] = [dic['ALPHAS'][0]]*R_size

    if type(dic['ADJ_PHIS']) != list:
        dic['ADJ_PHIS'] = [dic['ADJ_PHIS']]*R_size
    elif len(dic['ADJ_PHIS']) != R_size:
        dic['ADJ_PHIS'] = [dic['ADJ_PHIS'][0]]*R_size
        
    if type(dic['BETAS']) != list:
        dic['BETAS'] = [dic['BETAS']]*R_size
    elif len(dic['BETAS']) != R_size:
        dic['BETAS'] = [dic['BETAS'][0]]*R_size
        
    if type(dic['ADJ_OMEGAS']) != list:
        dic['ADJ_OMEGAS'] = [dic['ADJ_OMEGAS']]*R_size
    elif len(dic['ADJ_OMEGAS']) != R_size:
        dic['ADJ_OMEGAS'] = [dic['ADJ_OMEGAS'][0]]*R_size

    if type(dic['MS']) != list:
        dic['MS'] = [dic['MS']]*R_size
    elif len(dic['MS']) != R_size:
        dic['MS'] = [dic['MS'][0]]*R_size

    if type(dic['K_SS']) != list:
        dic['K_SS'] = [dic['K_SS']]*R_size
    elif len(dic['K_SS']) != R_size:
        dic['K_SS'] = [dic['K_SS'][0]]*R_size
        
    if type(dic['D_SS']) != list:
        dic['D_SS'] = [dic['D_SS']]*R_size
    elif len(dic['D_SS']) != R_size:
        dic['D_SS'] = [dic['D_SS'][0]]*R_size
    
    
    return dic

def writeHyperparametersToCSV(hyperparameterDict):
    """ Writes the resulting wanted hyperparameters (contained in the Python dictionary) to a hyperparameter .CSV file which the Unity synchrony simulator loads in and assigns to its internal hyperparameters before running a simulation run. """
    
    # Writing the .CSV header containing covariate / hyperparameter names, followed by corresponding wanted covariates / hyperparameters in the next row in a .CSV file:
    
    hyperparameterHeaderList = listifyHyperparameterHeaderFromDictionary(hyperparameterDict)
    hyperparameterCovariatesList = listifyHyperparameterCovariatesFromDictionary(hyperparameterDict)
    
    with open('wantedHyperparametersForSimulationRun.csv', 'w', newline='') as csvfile:
        spamwriter = csv.writer(csvfile, delimiter=';') # Kan bruke de siste to parameterne: quotechar='|', quoting=csv.QUOTE_MINIMAL
        spamwriter.writerow(hyperparameterHeaderList)
        spamwriter.writerow(hyperparameterCovariatesList)

def listifyHyperparameterHeaderFromDictionary(dictContainingCollectiveAndIndividualHyperparams):
    """ First, simply converting dictionary keys that aren't lists into a list.
        Second, chronologically appending the individual hyperparam. headers that have suffixes corresponding to their robot's agentID. 
                                """
    hyperparamHeaderList = []
    
    # Appending the first and collective / environment related hyperparameter header names:
    for key in dictContainingCollectiveAndIndividualHyperparams:
        value = dictContainingCollectiveAndIndividualHyperparams[key]
        if type(value) != list:
            hyperparamHeaderList.append(key)
    
    # Appending the latter, clustered, and individual / agent / robot related hyperparameter header names:
    for key in dictContainingCollectiveAndIndividualHyperparams:
        value = dictContainingCollectiveAndIndividualHyperparams[key]
        if type(value) == list:
            agentIDKeyStartStub = key[:-1] # just removing the 's'.
            
            values = value # e.g. ALPHAS = [0.4, 0.2, 0.3]
            for i in range(1, len(values)+1):
                agentIDHyperparameterHeaderValue = agentIDKeyStartStub + "_" + str(i) # e.g. ALPHA_1, ALPHA_2, ALPHA_3
                hyperparamHeaderList.append(agentIDHyperparameterHeaderValue)
    

    return hyperparamHeaderList

def listifyHyperparameterCovariatesFromDictionary(dictContainingCollectiveAndIndividualHyperparams):
    # byttUtPunktumerMedKommaer(list(hyperparameterDict.values()))
    hyperparamCovariateList = []
    
    # Appending the first and collective / environment related hyperparameter covariates:
    for key in dictContainingCollectiveAndIndividualHyperparams:
        value = dictContainingCollectiveAndIndividualHyperparams[key]
        if type(value) != list:
            hyperparamCovariateList.append(value)
    
    # Appending the latter, clustered, and individual / agent / robot related hyperparameter header names:
    for key in dictContainingCollectiveAndIndividualHyperparams:
        value = dictContainingCollectiveAndIndividualHyperparams[key]
        if type(value) == list:
            values = value # e.g. ALPHAS = [0.4, 0.2, 0.3]
            for agentIDValue in values:
                hyperparamCovariateList.append(agentIDValue)
    

    return byttUtPunktumerMedKommaer(hyperparamCovariateList)


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
    if silent == True:
        os.system("SynchronySimulatorRun.exe -batchmode -nographics")
    else:
        os.system("SynchronySimulatorRun.exe")