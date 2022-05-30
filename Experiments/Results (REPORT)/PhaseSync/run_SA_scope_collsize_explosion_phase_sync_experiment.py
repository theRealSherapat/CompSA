from experiment_running_utils import *

def main():
    xtickValues = [2, 3, 6, 10, 15, 25, 40, 50, 75, 100, 150, 200, 350, 500, 750, 1000, 1250]
    
    # Global SA scope:
    assignNonDefaultHyperparameters(collsize=collSize_val, k_ss=10000000, d_ss=0, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
    
    runNumberOfSimulationRuns(sampSize)
    
    extraXTickValues = [1600, 3200, 6400, 12800, 25600]
    
    for collSize_val in extraXTickValues:
        # k_s=1 nearest neighbour SA scope:
        assignNonDefaultHyperparameters(collsize=collSize_val, k_ss=1, d_ss=0, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
        
        runNumberOfSimulationRuns(sampSize)
        
        # Decent d_s=75 radial SA scope:
        assignNonDefaultHyperparameters(collsize=collSize_val, k_ss=0, d_ss=75, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
        
        runNumberOfSimulationRuns(sampSize)
    
        # Global SA scope:
        assignNonDefaultHyperparameters(collsize=collSize_val, k_ss=10000000, d_ss=0, adj_phis=0, adj_omegas=0, alphas=0.8, betas=0)
        
        runNumberOfSimulationRuns(sampSize)
    


if __name__ == "__main__":
    """ Fully automatic experiments script (although homogenous and saving to one synchronyDataset.csv file). """

    main()