from experiment_running_utils import *

def main():
    collsizes = [400, 800, 1600, 3200, 6400, 12800]
    
    for collSize_val_ind, collSize_val in enumerate(collsizes):
        # k_s/(|R|-1)=0.1 nearest neighbour SA scope:
        k_ss_values_rounded = int(round(0.1*(collSize_val-1)))
        assignNonDefaultHyperparameters(collsize=collSize_val, k_ss=k_ss_values_rounded, d_ss=0, adj_phis=1, adj_omegas=1, alphas=0.2, betas=0.7)
    
        runNumberOfSimulationRuns(sampSize)
        
        # k_s/(|R|-1)=0.8 nearest neighbour SA scope:
        k_ss_values_rounded = int(round(0.8*(collSize_val-1)))
        assignNonDefaultHyperparameters(collsize=collSize_val, k_ss=k_ss_values_rounded, d_ss=0, adj_phis=1, adj_omegas=1, alphas=0.2, betas=0.7)
        
        runNumberOfSimulationRuns(sampSize)
        
        # Decent d_s=265.3 radial SA scope:
        assignNonDefaultHyperparameters(collsize=collSize_val, k_ss=0, d_ss=265.3, adj_phis=1, adj_omegas=1, alphas=0.2, betas=0.7)
        
        runNumberOfSimulationRuns(sampSize)
    
        # Global SA scope:
        assignNonDefaultHyperparameters(collsize=collSize_val, adj_phis=1, adj_omegas=1, alphas=0.2, betas=0.7)
        
        runNumberOfSimulationRuns(sampSize)
    


if __name__ == "__main__":
    """ Fully automatic experiments script (although homogenous and saving to one synchronyDataset.csv file). """

    main()