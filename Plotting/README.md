# Plotting Synchrony-plots from Synchrony-simulation(s)

Requirements/dependencies: Having Python 3.x, numpy, and matplotlib installed, e.g. by writing for the libraries in the command window e.g.:

> pip install numpy

There are currently **three types of plots** you can plot with the Python-scripts in this folder:

## Performance-measure plots (or "Node-firing"-plots)

After having run one or more Synchrony simulation-runs in Unity, you can plot the corresponding performance-measure plot by running the following command when being in the _Plotting/_-folder:

> py plot_PerformanceMeasurePlot_for_SimRun.py _SimRun_ _saveFigurePlease_

where _SimRun_ for the first (or zero'th) simulation-run is 0, the second (or one'th) is 1, etc., and _saveFigurePlease_ simply is a flag for saving plot to .PDF or not ('1' for yes and elsewise for 'no'). So if we simply want to see the performance-measure plot for the first run we ran in Unity, the command would look like this:

> py plot_PerformanceMeasurePlot_for_SimRun.py 0 0

or conversely

> py plot_PerformanceMeasurePlot_for_SimRun.py 0 1

if we wanted to save the figure to .PDF.

## Phase- & Frequency- plots

Similarly to before, after having run one or more Synchrony simulation-runs in Unity, you can plot the corresponding phase- & frequency- plot by running the following command when being in the _Plotting/_-folder:

> py plot_PhaseFrequencyPlot_for_SimRun.py _SimRun_ _saveFigurePlease_

where _SimRun_ and _saveFigurePlease_ here means the same as above. So for simply seeing the phase- & frequency- plot corresponding to the first run we ran in Unity—as well as the first performance-measure plot as obtained above—we have to run:

> py plot_PhaseFrequencyPlot_for_SimRun.py 0 0


## Synchrony Evolution plots

Similarly to before, after having run one or more Synchrony simulation-runs in Unity, you can plot the corresponding Synchrony-evolution plots per run by running the following command when being in the _Plotting/_-folder:

> py plot_SynchronyEvolutionPlot_for_SimRun.py _SimRun_ _saveFigurePlease_

where _SimRun_ and _saveFigurePlease_ here means the same as above.