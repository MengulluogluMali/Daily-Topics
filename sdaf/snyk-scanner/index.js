const express = require('express');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const app = express();
app.use(express.json());

const SCANS_DIR = '/scans';

app.post('/scan', (req, res) => {
    const { scan_id, repo_url } = req.body;

    if (!scan_id || !repo_url) {
        return res.status(400).json({ error: 'scan_id and repo_url are required' });
    }

    try {
        const scanPath = path.join(SCANS_DIR, `scan_${scan_id}`);
        
        console.log(`[Scanner] Processing scan_id: ${scan_id} for URL: ${repo_url}`);
        
        // 1. Clone the repository into the shared /scans volume
        if (!fs.existsSync(scanPath)) {
            fs.mkdirSync(scanPath, { recursive: true });
        }
        
        console.log(`[Scanner] Cloning ${repo_url} into ${scanPath}`);
        // Only doing a shallow clone for speed
        execSync(`git clone --depth 1 ${repo_url} .`, { cwd: scanPath, stdio: 'inherit' });

        // 2. Perform the Snyk scan. Output as JSON.
        // The image snyk/snyk:node ships with snyk binary globally available
        console.log(`[Scanner] Running snyk test...`);
        let snykOutput = '';
        try {
            // Snyk test returns non-zero exit code if vulnerabilities are found
            const outputBuffer = execSync(`snyk test --json`, { cwd: scanPath });
            snykOutput = outputBuffer.toString();
        } catch (error) {
            // Snyk throws when vulnerabilities exist (exit code 1 or 2)
            if (error.stdout) {
                snykOutput = error.stdout.toString();
            } else {
                throw error;
            }
        }

        // 3. Parse and return results
        const jsonResults = JSON.parse(snykOutput);
        console.log(`[Scanner] Scan finished. Found items.`);
        
        // Optionally, clean up the cloned code if not needed to preserve space
        execSync(`rm -rf ${scanPath}`);

        return res.json({
            status: 'success',
            results: jsonResults
        });

    } catch (err) {
        console.error('[Scanner] Error executing scan:', err.message);
        res.status(500).json({ error: err.message });
    }
});

const PORT = 3002;
app.listen(PORT, () => {
    console.log(`[Scanner] Wrapper service listening on port ${PORT}`);
});
