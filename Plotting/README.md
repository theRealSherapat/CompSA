# Plotting Synchrony-plots from Synchrony-simulation(s)

Requirements/dependencies: Having Python 3.x, numpy, and matplotlib installed, e.g. by writing for the libraries:

> pip install numpy

There are currently **two types of plots** you can plot with the Python-scripts in this folder:

## Performance-measure plots (or "Node-firing"-plots)

After having run one or more Synchrony simulation-runs in Unity, you can plot the corresponding performance-measure plot by running the following command when being in the _Plotting/_-folder:

> py plot_PerformanceMeasurePlot_for_SimRun.py _SimRun_

where _SimRun_ for the first (or zero'th) simulation-run is 0, the second (or one'th) is 1, etc. So if we want to see the performance-measure plot for the first run we ran in Unity, the command would look like this:

> py plot_PerformanceMeasurePlot_for_SimRun.py 0


## Phase- & Frequency- plots

Similarly to before, after having run one or more Synchrony simulation-runs in Unity, you can plot the corresponding phase- & frequency- plot by running the following command when being in the _Plotting/_-folder:

> py plot_PhaseFrequencyPlot_for_SimRun.py _SimRun_

where _SimRun_ here means the same as above. So for seeing the phase- & frequency- plot corresponding to the first run we ran in Unity—as well as the first performance-measure plot as obtained above—we have to run:

> py plot_PhaseFrequencyPlot_for_SimRun.py 0
