from experiment_running_utils import *
import numpy as np

mostInterestingCollsizeForHeterogenousSAScopeExperiment = 15

mostInterestingAndValidMinimalSAScopeKSValue = round(0.4*(mostInterestingCollsizeForHeterogenousSAScopeExperiment-1)) # E.g. 40% of closest neighbours will be defined as the 'minimal SA scope' in this experiment.
collsizeSpawningRadius = (mostInterestingCollsizeForHeterogenousSAScopeExperiment / 6) * 5.7 + 4
mostInterestingAndValidRadialSAScopeDSValue = 0.17*2*collsizeSpawningRadius
GLOBAL_VALUE = 10000000

def main():
    # 30 RUNS MED DET MINST OPPMERKSOMME ROBOT KOLLEKTIVET (m, r, g) = (1.0, 0.0, 0.0):
    k_ss_values, d_ss_values = getCollsizeSAScopeRatioImplementationValues(mostInterestingCollsizeForHeterogenousSAScopeExperiment, (1.0, 0.0, 0.0))
    assignNonDefaultHyperparameters(collsize=mostInterestingCollsizeForHeterogenousSAScopeExperiment, k_ss=k_ss_values, d_ss=d_ss_values, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
    runNumberOfSimulationRuns(sampSize)
    
        # BARTRE FOR TESTING:
    # print("\nk_ss_values with len",len(k_ss_values),":",k_ss_values, " , and d_ss_values with len",len(d_ss_values),":",d_ss_values)
    
    
    k_ss_values, d_ss_values = getCollsizeSAScopeRatioImplementationValues(mostInterestingCollsizeForHeterogenousSAScopeExperiment, (0.65, 0.25, 0.1))
    assignNonDefaultHyperparameters(collsize=mostInterestingCollsizeForHeterogenousSAScopeExperiment, k_ss=k_ss_values, d_ss=d_ss_values, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
    runNumberOfSimulationRuns(sampSize)
    
    # BARTRE FOR TESTING:
    # print("\nk_ss_values with len",len(k_ss_values),":",k_ss_values, " , and d_ss_values with len",len(d_ss_values),":",d_ss_values)
    
    # 30 RUNS MED ET LITT MER OPPMERKSOMT ROBOT KOLLEKTIV (m, r, g) = (0.4, 0.4, 0.2):
    k_ss_values, d_ss_values = getCollsizeSAScopeRatioImplementationValues(mostInterestingCollsizeForHeterogenousSAScopeExperiment, (0.4, 0.4, 0.2))
    assignNonDefaultHyperparameters(collsize=mostInterestingCollsizeForHeterogenousSAScopeExperiment, k_ss=k_ss_values, d_ss=d_ss_values, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
    runNumberOfSimulationRuns(sampSize)
    
    # BARTRE FOR TESTING:
    # print("\nk_ss_values with len",len(k_ss_values),":",k_ss_values, " , and d_ss_values with len",len(d_ss_values),":",d_ss_values)
    
    k_ss_values, d_ss_values = getCollsizeSAScopeRatioImplementationValues(mostInterestingCollsizeForHeterogenousSAScopeExperiment, (0.33, 0.33, 0.33))
    assignNonDefaultHyperparameters(collsize=mostInterestingCollsizeForHeterogenousSAScopeExperiment, k_ss=k_ss_values, d_ss=d_ss_values, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
    runNumberOfSimulationRuns(sampSize)
    
    # BARTRE FOR TESTING:
    # print("\nk_ss_values with len",len(k_ss_values),":",k_ss_values, " , and d_ss_values with len",len(d_ss_values),":",d_ss_values)
    
    # 30 RUNS MED ET MER OPPMERKSOMT ROBOT KOLLEKTIV (m, r, g) = (0.2, 0.4, 0.4):
    k_ss_values, d_ss_values = getCollsizeSAScopeRatioImplementationValues(mostInterestingCollsizeForHeterogenousSAScopeExperiment, (0.2, 0.4, 0.4))
    assignNonDefaultHyperparameters(collsize=mostInterestingCollsizeForHeterogenousSAScopeExperiment, k_ss=k_ss_values, d_ss=d_ss_values, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
    runNumberOfSimulationRuns(sampSize)
    
    # BARTRE FOR TESTING:
    # print("\nk_ss_values with len",len(k_ss_values),":",k_ss_values, " , and d_ss_values with len",len(d_ss_values),":",d_ss_values)
    
    k_ss_values, d_ss_values = getCollsizeSAScopeRatioImplementationValues(mostInterestingCollsizeForHeterogenousSAScopeExperiment, (0.1, 0.25, 0.65))
    assignNonDefaultHyperparameters(collsize=mostInterestingCollsizeForHeterogenousSAScopeExperiment, k_ss=k_ss_values, d_ss=d_ss_values, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
    runNumberOfSimulationRuns(sampSize)
    
    # BARTRE FOR TESTING:
    # print("\nk_ss_values with len",len(k_ss_values),":",k_ss_values, " , and d_ss_values with len",len(d_ss_values),":",d_ss_values)
    
    # 30 RUNS MED DET MEST OPPMERKSOMNE ROBOT KOLLEKTIVET (m, r, g) = (0.0, 0.0, 1.0):
    k_ss_values, d_ss_values = getCollsizeSAScopeRatioImplementationValues(mostInterestingCollsizeForHeterogenousSAScopeExperiment, (0.0, 0.0, 1.0))
    assignNonDefaultHyperparameters(collsize=mostInterestingCollsizeForHeterogenousSAScopeExperiment, k_ss=k_ss_values, d_ss=d_ss_values, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
    runNumberOfSimulationRuns(sampSize)
    
    # BARTRE FOR TESTING:
    # print("\nk_ss_values with len",len(k_ss_values),":",k_ss_values, " , and d_ss_values with len",len(d_ss_values),":",d_ss_values)
    
def getCollsizeSAScopeRatioImplementationValues(collsize, minRadiGlobalRatio):
    """ Returns a scalar (if homogenous agents) or lists (if heterogenous agents) values for the SA scopes, corresponding to the given minRadiGlobalRatio. """
    
    k_ss_and_d_ss_vals = []
    
    agentsWithMinimalSAScope = round(collsize*minRadiGlobalRatio[0])
    agentsWithRadialSAScope = round(collsize*minRadiGlobalRatio[1])
    agentsWithGlobalSAScope = round(collsize*minRadiGlobalRatio[2])
    
    # minScopeCounter = 0
    # radScopeCounter = 0
    # globScopeCounter = 0
    for agentID in range(collsize):
        if agentID < agentsWithMinimalSAScope:
            k_ss_and_d_ss_vals.append([mostInterestingAndValidMinimalSAScopeKSValue, 0])
        elif agentID < (agentsWithMinimalSAScope + agentsWithRadialSAScope):
            k_ss_and_d_ss_vals.append([0, mostInterestingAndValidRadialSAScopeDSValue])
        else:
            k_ss_and_d_ss_vals.append([GLOBAL_VALUE, 0])
    
    k_ss_and_d_ss_arr = np.array(k_ss_and_d_ss_vals)
    
    # SHUFFLE 'k_ss_and_d_ss_vals':
    np.random.shuffle(k_ss_and_d_ss_arr)
    
    
    return list(k_ss_and_d_ss_arr[:,0]), list(k_ss_and_d_ss_arr[:,1])
    

def getSpawnRadius(collsize):
    
    return (collsize/6) * 5.7 + 4


if __name__ == "__main__":
    """ Fully automatic experiments script (although homogenous and saving to one synchronyDataset.csv file). """

    main()
