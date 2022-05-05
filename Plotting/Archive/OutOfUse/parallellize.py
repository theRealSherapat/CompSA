import os
import multiprocessing as mp

def main():
    sampleruns = 5
    
    pool = mp.Pool(mp.cpu_count()) # evt. cpu_count()-1 hvis du vil la en av prosessorene være ledig
    
    pool.map(run_synchrony_simulator_executable, [i for i in range(sampleruns)])
    
    pool.close()
    

def run_synchrony_simulator_executable(iteration_index):
    os.system("SynchronySimulatorRun.exe -batchmode -nographics")

if __name__ == "__main__":
    main()