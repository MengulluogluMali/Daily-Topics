const express = require('express');
const { Pool } = require('pg');
const axios = require('axios');
const cors = require('cors');

const app = express();
app.use(express.json());
app.use(cors());

// Database connection
const pool = new Pool({
  connectionString: process.env.DATABASE_URL
});

pool.on('error', (err) => {
  console.error('Unexpected error on idle client', err);
  process.exit(-1);
});

// Create a new scan
app.post('/api/scans', async (req, res) => {
  const { repo_url } = req.body;

  if (!repo_url) {
    return res.status(400).json({ error: 'repo_url is required' });
  }

  const client = await pool.connect();
  try {
    // 1. Insert pending record
    const result = await client.query(
      'INSERT INTO scans(project_url, status) VALUES($1, $2) RETURNING id',
      [repo_url, 'pending']
    );
    const scanId = result.rows[0].id;
    console.log(`[Backend] Created scan record ID: ${scanId} for URL ${repo_url}`);

    // Wait on DB, reply immediately to user and trigger scan asynchronously
    res.status(202).json({
      message: 'Scan triggered successfully',
      scan_id: scanId
    });

    runScan(scanId, repo_url).catch(err => {
        console.error(`[Backend] Background scan error for ID ${scanId}:`, err);
    });

  } catch (err) {
    console.error('[Backend] DB Error:', err);
    res.status(500).json({ error: 'Database error' });
  } finally {
    client.release();
  }
});

// Async function to orchestrate the scan
async function runScan(scanId, repoUrl) {
    const client = await pool.connect();
    try {
        await client.query('UPDATE scans SET status = $1 WHERE id = $2', ['scanning', scanId]);
        
        console.log(`[Backend] Triggering snyk-scanner for ID ${scanId}...`);
        
        // Call the snyk-scanner service
        const scannerUrl = process.env.SNYK_SCANNER_URL || 'http://snyk-scanner:3002';
        const response = await axios.post(`${scannerUrl}/scan`, {
            scan_id: scanId,
            repo_url: repoUrl
        }, { timeout: 300000 }); // 5 minute timeout
        
        const scanResults = response.data.results;
        
        // Update DB with results
        await client.query(
            'UPDATE scans SET status = $1, results = $2 WHERE id = $3',
            ['completed', JSON.stringify(scanResults), scanId]
        );
        console.log(`[Backend] Scan ${scanId} completed successfully.`);
        
    } catch (err) {
        console.error(`[Backend] Error running scan ${scanId}:`, err.message);
        
        let errorMsg = err.message;
        if (err.response && err.response.data) {
           errorMsg = JSON.stringify(err.response.data);
        }

        await client.query(
            'UPDATE scans SET status = $1, results = $2 WHERE id = $3',
            ['failed', JSON.stringify({ error: errorMsg }), scanId]
        );
    } finally {
        client.release();
    }
}

// Get scan results
app.get('/api/scans/:id', async (req, res) => {
    try {
        const { rows } = await pool.query('SELECT * FROM scans WHERE id = $1', [req.params.id]);
        if (rows.length === 0) return res.status(404).json({ error: 'Scan not found' });
        res.json(rows[0]);
    } catch (err) {
        res.status(500).json({ error: 'Database error' });
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`[Backend] Service running on port ${PORT}`);
});
