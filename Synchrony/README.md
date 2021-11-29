# Achieving Decentralized Synchronization (cf. K. Nymoen, 2014) in Unity
This folder is partly an image of a Unity project, having some project-files (mostly Unity-generated, binary and large) _.gitignored_.

## Future work TODOs:
* Implement Frequency-adjustment
* Produce and record synchrony-plots/-figures/-statistics from the simulations (Cf. the 'Essay ferdig Recall'-note on the reMarkable).
* Think about the Python-switch (and ASK FRANK).

## Future Clean-Ups in the code:
* Make the code so that anyone can git-clone the project and hit run in Unity (without having to fix a lot in the Inspector, and assign audio-files there etc.)

### ONLY IF TIME- Future work TODOs:
* Study timesteps and deltaTime to account for high frequencies
* Finish the Dr. Squiggles robots Simulation-wishes (on the reMarkable)

## Discrepancies between this implementation, and the ones described in K. Nymoen et al.'s "Firefly"-paper:
* The _refraction period_ mentioned in the paper, modelling physical audio traversal, is not included in the implementation.
  + If _refraction-periods_ are implemented:
	- Apply the special case that the Error-score = 0 if a fire event is received within the refractory period.
    - Double-check that the refraction-period and phase-adjustments are being done properly (w/printouts of Time.time e.g.)
