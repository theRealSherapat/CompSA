from experiment_running_utils import *

def main():
    collSizes = [3, 15, 50, 200]
    d_sValues = getAllDSValues(collSizes)

    for collSize_val in collSizes:
        for d_sValue in d_sValues:
            assignNonDefaultHyperparameters(collsize=collSize_val, d_ss=d_sValue, k_ss=0, alphas=0.8, betas=0)
            runNumberOfSimulationRuns(sampSize)
        
 
def getAllDSValues(collsizes):
    slingringsMonn = 4.0
    agentWidth = 5.6568 # = sqrt(4^2+4^2)
    spawnRadius = (collsizes[-1]/6.0)*agentWidth + slingringsMonn # Simply an empirical model of the necessary space the agents need to spawn. Or just a guess I guess.
    
    min_d_sValue = agentWidth
    max_d_sValue = 2*spawnRadius

    d_sValues = np.linspace(min_d_sValue, max_d_sValue, 20)
    
    
    return d_sValues


if __name__ == "__main__":
    """ Fully automatic experiments script. """

    main()