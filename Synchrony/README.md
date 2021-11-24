# Achieving Decentralized Synchronization (cf. K. Nymoen, 2014) in Unity
This folder is partly an image of a Unity project, having some project-files (mostly Unity-generated, binary and large) _.gitignored_.

## Future work TODOs:
* Implement Frequency-adjustment <span style="color:gray">(as in _UiO_/_MSc_/_Logs_/_Simulations_/_Frequency adjustment_)</span>.
* Produce and record synchrony-plots/-figures/-statistics from the simulations <span style="color:gray">(Cf. the 'Essay ferdig Recall' and 'Mid-November talk' Møte-note on the reMarkable at _UiO_/_MSc_/_Møter_/_ROBIN_/_Kyrre_)</span>.

### (ONLY IF TIME)-'Future work TODOs':
* Connect to HPC clusters <span style="color:gray">(either (from 'Mid-November Talk' Møte-Recall) ROBIN-HPC, Rudolph, and lastly eventually (for the final experiments that I want to run many times after each other) the super-computers. Or, as I thought before the 'Mid-November Talk', the ml-clusters used in IN5400, or some others - cf. Robin Wiki and/or ask others at ROBIN)</span>.
*Cozifying/Prettifying/Harmonizing(with sound) of the Squiggles-scene:*
* Finish the Dr. Squiggles Scene Simulation-wishes <span style="color:gray">(on the reMarkable)</span>.
*Achieving Realistic Simulation:*
* Study timesteps and deltaTime to assure high frequencies don't break the simulation/matrix.
* Remove Discrepancy/"reality-gap" <span style="color:gray">(as mentioned below)</span>, and introduce the Refractory Period t_ref.

## Discrepancies between this implementation, and the ones described in K. Nymoen et al.'s "Firefly"-paper:
* The *refraction period* mentioned in the paper, modelling real-world & physical audio traversal/delay, is not included in the Unity-implementation.
  + If *refraction-periods* are implemented:
    - Double-check that the refraction-period and phase-adjustments are being done properly <span style="color:gray">(w/printouts of Time.time e.g.)</span>.
