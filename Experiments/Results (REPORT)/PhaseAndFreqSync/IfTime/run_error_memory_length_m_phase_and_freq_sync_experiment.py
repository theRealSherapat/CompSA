from experiment_running_utils import *

def main():
    collsizes = [2, 3, 6, 10, 15, 25, 40, 50, 75, 100, 150, 200, 350, 500, 750, 1000, 1250, 1500, 2000]
    m_values = [2, 5, 10, 20]
    
    for collSize_val in collsizes:
        for memory_length_m_value in m_values:
            assignNonDefaultHyperparameters(collsize=collSize_val, ms=memory_length_m_value, adj_phis=1, adj_omegas=1, alphas=0.2, betas=0.7)
            
            runNumberOfSimulationRuns(sampSize)
    
    


if __name__ == "__main__":
    """ Fully automatic experiments script (although homogenous and saving to one synchronyDataset.csv file). """

    main()