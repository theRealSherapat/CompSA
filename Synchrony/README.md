# Achieving Decentralized Synchronization (cf. K. Nymoen, 2014) in Unity
This folder is partly an image of a Unity project, having some project-files (mostly Unity-generated, binary and large) _.gitignored_.

## Future work TODOs:
* Implement Frequency-adjustment
* Implement the same functionality in Unity with Python through 'ML-agents', with necessary Python-packages automatically downloaded according to a requirements-file in a virtual environment (pyenv).
* Produce and record synchrony-plots/-figures/-statistics from the simulations (Cf. the 'Essay ferdig Recall'-note on the reMarkable)

## Future Clean-Ups in the code:
* Make the code so that anyone can git-clone the project and hit run in Unity (without having to fix a lot in the Inspector, and assign audio-files there etc.)

### ONLY IF TIME- Future work TODOs:
* Study timesteps and deltaTime to account for high frequencies
* Finish the Dr. Squiggles robots Simulation-wishes (on the reMarkable)

## Discrepancies between this implementation, and the ones described in K. Nymoen et al.'s "Firefly"-paper:
* The _refraction period_ mentioned in the paper, modelling physical audio traversal, is not included in the implementation.
  + If _refraction-periods_ are implemented:
    - Double-check that the refraction-period and phase-adjustments are being done properly (w/printouts of Time.time e.g.)
