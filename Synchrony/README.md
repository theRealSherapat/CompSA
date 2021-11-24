# Achieving Decentralized Synchronization (cf. K. Nymoen, 2014) in Unity
This folder is partly an image of a Unity project, having some project-files (mostly Unity-generated, binary and large) ".gitignored".

## 'Future work TODOs':
* Implement Frequency-adjustment (as in _UiO_/_MSc_/_Logs_/_Simulations_/_Frequency adjustment_).
* Produce and record synchrony-plots/-figures/-statistics from the simulations (Cf. the 'Essay ferdig Recall' and 'Mid-November talk' Møte-note on the reMarkable at _UiO_/_MSc_/_Møter_/_ROBIN_/_Kyrre_).

### (ONLY IF TIME)-'Future work TODOs':
* Connect to HPC clusters (either (from 'Mid-November Talk' Møte-Recall) ROBIN-HPC, Rudolph, and lastly eventually (for the final experiments that I want to run many times after each other) the super-computers. Or, as I thought before the 'Mid-November Talk', the ml-clusters used in IN5400, or some others - cf. Robin Wiki and/or ask others at ROBIN).

__Cozifying/Prettifying/Harmonizing(with sound) of the Squiggles-scene:__
* Finish the Dr. Squiggles Scene Simulation-wishes (on the reMarkable).

__Achieving Realistic Simulation:__
* Study timesteps and deltaTime to assure high frequencies don't break the simulation/matrix.
* Remove Discrepancy/"reality-gap" (as mentioned below), and introduce the Refractory Period _t_ref_.

## Discrepancies between this implementation, and the ones described in K. Nymoen et al.'s "Firefly"-paper:
* The __refraction period__ mentioned in the paper, modelling real-world & physical audio traversal/delay, is not included in the Unity-implementation.
  + If __refraction-periods__ are implemented:
    - Double-check that the refraction-period and phase-adjustments are being done properly (w/printouts of Time.time e.g.).
