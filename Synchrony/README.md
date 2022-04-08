# Achieving _decentralized harmonic synchronization_ in a musical robot collective in Unity
Unity Version: 2021.2.0f1.

This folder contains the Unity project-code for the developed Synchrony-Simulator. It is an image of a full Unity-project, although having some project-files (mostly Unity-generated, binary and large) _.gitignored_.

## Polishing- & Finishing-TODOs (if time permits)
* See if modelling non-ideal signals in Unity leads to 'Kristian-over-Mirollo-Strogatz'-superiority in phase-synchronization (e.g. by pretending to only "hear" the signal after a bit longer than the duration of the audio-pulse's waveform, after having been NotifyAgent()'ed).
	+ C.f. 'Klar progress møte-recall.'
* Prøv å flytte FixedUpdate()-funksjonaliteten inn i Update(), for å sjekke om ting fungerer bedre da.
* Try increasing the numerical accuracy by using 'double's instead of 'float's.
* Fullfør reMarkable.'Logs & History'/'Synch.-Simulation'/'Squiggles in Unity' . 'Prettify the scene.'
	+ Kanskje heller ha et fint og fargerikt grid-gulv? Eller kanskje ikke hvis robotene ikke skal bevege på seg?
* Fullfør reMarkable.'Logs & History'/'Synch.-Simulation'/'Squiggles in Unity' . 'Instruments Played Based on Frequencies.'