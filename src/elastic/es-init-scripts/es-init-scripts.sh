#!/bin/bash
set -e

echo "Creating initial OTEL index and write alias..."

curl -s -X PUT "http://localhost:9200/otel-logs-000001" -H 'Content-Type: application/json' -d'
{
  "aliases": {
    "otel-logs-write": {
      "is_write_index": true
    }
  }
}
'

echo "Index and alias created."

# Optional: create ILM policy
curl -s -X PUT "http://localhost:9200/_ilm/policy/otel-logs-policy" -H 'Content-Type: application/json' -d'
{
  "policy": {
    "phases": {
      "hot": {
        "actions": {
          "rollover": {
            "max_age": "1d",
            "max_docs": 100000
          }
        }
      }
    }
  }
}
'

# Attach ILM to index template
curl -s -X PUT "http://localhost:9200/_index_template/otel-logs-template" -H 'Content-Type: application/json' -d'
{
  "index_patterns": ["otel-logs-*"],
  "template": {
    "settings": {
      "index.lifecycle.name": "otel-logs-policy",
      "index.lifecycle.rollover_alias": "otel-logs-write"
    },
    "mappings": {
      "properties": {
        "observedTimestamp": { "type": "date" },
        "body": { "type": "text" },
        "severity": { "type": "keyword" }
      }
    }
  }
}
'

echo "ILM policy and index template configured."
