# MSc Thesis document TODOs

Remember:
* "Less is more." — experience and William Strunk Jr.
* "Don't overkill it." — experience (crazy and many color-codes)
* "Ikke drible eller lure deg selv." — overdreven bruk av "clarifying markers" og fotball


## Specific TODOs
* Ordne med en annen farge på kommentarer i .tex-filer i notepad++.
* Gå igjennom 'notert-på reMarkable Thesis-document(s)', 'implementation_chapter_pre_first_draft.tex' og 'Essay .tex'-fila, så rett på eller hent kritikk, INKL.:[]?'s, og concerns.
* Execute 'GJØR: []'-clauses possible to execute now (remember Abe Lincoln).




## Polishing- & Finishing-TODOs
* Font-families, -styles, and -sizes (by e.g. using the packages 'sectsty' or 'titlesec' — but preferably just LaTeX's standard font-manipulation if possible):
	+ Make Section- & Chapter-headlines (and possibly the 2nd thesis-title, like in the Overleaf-IFI-standard-document) bold-face.
	+ Ensure that the Thesis-title isn't inferior and smaller perhaps than the Chapter/Section-headlines.
	+ Use a good-looking font-family. Previously looked at font-families are:
		- 'Aldus Nova Book' by Linotype (found through myfonts.com/WhatTheFont with a sample from Tønnes's master).
		- 'bold Adobe Times' a.k.a. '\fontfamily{ptm}'  (found through the 'sectsty'-package docs on page 3).
		- 'Libre Baskerville' a.k.a. '\usepackage{librebaskerville} \usepackage[T1]{fontenc}' (found at tug.org/FontCatalogue/seriffonts.html).
	+ Be consistent about using italization on variables and words of emphasis, but not on operators (in equations).
	+ Use bold face on Figure-captions (so that it's rather **Figure 4.1** instead of Figure 4.1)?
* Writing/text:
	+ Minimize the use of parentheses and '—'s. Jarka thought e.g. the use of a lot of '-'s and '—'s was confusing and Kyrre said it was not common in English language.
	+ Check present vs. past-tense.
	+ Check pronouns, and remove as many of them as possible (c.f. Jim Tørresen's masters-writing document).
	+ Run the master's document through PDF -> Word document -> Grammarly.
	+ Check the Harmonic Synchrony-conditions and consider whether to use bulletpoints, numbered list, or just 'Condition x:'.
* Citations and references throughout the master's report:
	+ Add the right citations/references where there are empty \cite{}-clauses.
	+ Ensure the right uses of \ref{}, so that 'Subsection \ref{}' doesn't e.g. refer to a Section or Paragraph.
	+ Høre med Kyrre om det er fryktelig nøye åssen referansene er skrevet opp, eller om Zoteros eksporterte .bib-filer er gode nok.
	+ Sjekke hva du har blitt fortalt om kilder (fra f.eks. de to master-ukene), og kildekompasset.no, og formater kildene dine ettersom.
* Plots and colors:
	+ (Så lenge det er profesjonelt og ikke noob (muligens forhør deg med Kyrre)) Marker alle plots (fra Python-scripts) slik at man utifra de kan se og vite om simulation-runnet var suksessfullt eller ikke (om agentene oppnådde harmonisk synkronitet eller ei):
		- E.g. k-linja (mål-linja) agentene må nå i 'SynchronyEvolution'-plottet.
	+ Read plot tips (incl. color-considerations, where my intuition is "neutral, RGB, and as contrastive colors as possible, or non-contrastive and similar if they are closely related"), and clean up and create existing figures and plots properly (if I'll want to keep said figure/plot).
	+ Color-check (matplotlib guides, color-guide/-paper)—so that one avoids e.g. rainbow-plots or green-to-red heatmaps, and eventually look at 'kodeeksempler' for how to plot nicely.
	+ Ensuring good enough DPI. 150 is too little, 300 is more standard. This can be changed in matplotlib.rcparams[figure.dpi] or something. But is 300 a bit little too? Or shouldn't it be higher (if it's not physically possible anyways)?
	+ Bruke gjennomgående og konsistente og tema-belyste farger i plotte-fargene, eller til noen du liker og synes ser proffe og fine ut (f.eks. turkis og sort som du liker).
* Frequency-spectra:
	+ Save the raw spectrogram-.PDF from Audacity, and rather downscale the resolution in e.g. Inkscape or something (like Kyrre suggested at the same meeting as below).
	+ Use Matplotlib.show() to construct vector-graphics axes, e.g. for the frequency-spectra you want to use in your thesis (as Mia suggested regarding creating vector-graphics axes during a collective meeting).
* Tables:
	+ Nicer zero-paddings (in the case of coloring cells, rows, or columns). See Tønnes's MSc-thesis at his Table 2.4 at (proper) page 13.
* Page layouts:
	+ Se til at alle figurene og tabellene er plassert der det gir mening for deg, ikke der det gir mening for LaTeX.
	+ Sørge for at List of Figures ser grei ut og ikke noob (husk å sende med forkortede []-parametre i figure-captions).
	
	
	
	
## ONLY IF TIME Polishing- & Finishing-TODOs
* Citations and references throughout the master's report:
	+ If really wanted and if time, changing to using the [NOR04]-style (first author(s), Normann e.g.) instead of [3]-style (cf. https://www.overleaf.com/learn/latex/Bibliography_management_in_LaTeX).
	+ When referring to footnotes in the text, add some spacing after it so that the reference looks good and not too crowded.	