CREATE TABLE scans (
    id SERIAL PRIMARY KEY,
    project_url VARCHAR(255) NOT NULL,
    status VARCHAR(50) NOT NULL, -- e.g., 'pending', 'scanning', 'completed', 'failed'
    results JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
