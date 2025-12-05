# Commit Automation Prompt

Use this prompt whenever you want to package local work into a pull request.

1. **Verify branch context**
   - Run `git status -sb` to confirm the current branch.
   - If the branch is `main`, create and switch to a new branch: `git checkout -b <feature-branch-name>`.
   - If you are already on a feature branch, continue without creating a new one.

2. **Craft and create the commit**
   - Review `git status` to confirm the staged and unstaged changes.
   - Stage everything that belongs in the change set (e.g., `git add .` or specific paths).
   - Write a detailed commit message summarizing the change motivation, scope, and impact.
   - Commit using `git commit -m "<concise-summary>: <short rationale and context>"`.

3. **Push to the remote**
   - Push the branch to origin, creating it upstream if needed: `git push -u origin HEAD`.

4. **Open a pull request**
   - Use the GitHub CLI or web interface to open the PR after push:
     - CLI: `gh pr create --fill` (or customize title/body as needed).
     - Web: open the compare view for the branch and create the PR manually.
   - Ensure the PR description captures the problem, solution, testing, and any follow-up tasks.

> 📝 **Tip:** If pre-push hooks, tests, or linters are required by the repository, run them before committing to keep the pipeline green.
