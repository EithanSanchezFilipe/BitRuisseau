## 24.11.25

- Clone: OK
- Journal (mis à jour) : KO: 1h30 le 17.11, 1h00 le 24.11
- Projet: OK
- User story: KO. Je ne vois aucune trace de maquettes, pourtant indispensables dans un projet de ce genre
- Git : OK
- Implémentation: Il y a un début, mais il est maigre

- Global: Mauvais départ, sur la base de ce que vous me donnez à voir. Corrigez le tir sans plus attendre SVP.

## 80%

- Réalisation du player standalone: OK
- Réalisation du player connecté:
  - Je ne vois pas de trace de de l'analyse fonctionnelle demandée par le CdC
  - Je vois du code relatif à la gestion des remotes, mais aucune trace dans l'interface
- Qualité du code:
  - Il est fonctioel mais ne suit pas les bonnes pratiques de séparation de responsabilité (présentation, logique métier, stockage, ...). Demandez à chatGPT, il est du même avis que moi.
  - Usage répété de la chaîne hardcodée "users"
- Maîtrise technique:
  - J'aimerais comprendre (=que vous m'expliquiez) le `override` dans Home.razor:37
  - Pourquoi faire des méthodes `async` dans AgentService ?
- Autonomie: OK
- Livraison: pas de journal de travail attaché à la release
- Journal de travail:
  - Grosse lacune sur les deux dernière semaines. Cela vient (entre autres) du fait que vous avez ajouté la durée et le statut sur la même ligne que le nom du commit, alors qu'il faut mettre cela sur la première ligne de la description (dans la boîte de description si vous utiliset Github Desktop, avec un second `-m "[20][DONE]"` en CLI) -> à corriger manuellement vu qu'on ne veut pas éditer les commits
- Git

  - .gitignore OK depuis le début
  - Nommage des commits: trop d'omission du scope dans le nommage
  - Comment se fait-il qu'il y aie des commits au nom d'Emma dans votre repo ?

- En résumé:
  - Quelques points à corriger
  - Adaptations + UI à faire pour le challenge
  - Une petite discussion à avoir
