from experiment_running_utils import *

def main():
    collSizes = [3, 15, 50, 200]
    coll_k_sValues = getAllKSValues(collSizes)

    for collsizeIndex, collSize_val in enumerate(collSizes):
        for k_s_value in coll_k_sValues[collsizeIndex]:
            assignNonDefaultHyperparameters(k_ss=k_s_value, collsize=collSize_val, alphas=0.8, betas=0)
            runNumberOfSimulationRuns(sampSize)
        
        
def getAllKSValues(collsizes):
    coll_k_sValues = []
    for collsize in collsizes:
        k_s_values = get_k_s_values(collsize)
        coll_k_sValues.append(k_s_values)
        
        
    return coll_k_sValues
    
def get_k_s_values(collsize):
    k_s_values_list = list(range(1,collsize))
    
    if len(k_s_values_list) > 20: # Max xtick number we want to plot
        k_s_values_list = [round(el) for el in np.linspace(k_s_values_list[0], k_s_values_list[-1], 20)]
    
    
    return k_s_values_list



if __name__ == "__main__":
    """ Fully automatic experiments script. """

    main()