import os

def main():
    """ REMEMBER TO SPLIT UP THE RESULTING DATASET AFTERWARDS!! """

    os.system("py run_phase_sync_d_s_SA_scope_tuning_experiment.py")
    
    os.system("py run_phase_and_freq_sync_d_s_SA_scope_tuning_experiment.py")

if __name__ == "__main__":
    """ Fully automatic experiments script. """

    main()