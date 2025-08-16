// Simple test to check if heatmap generation works locally

const testHeatmapGeneration = async () => {
    try {
        console.log('Testing heatmap generation...');
        
        const response = await fetch('http://localhost:5163/api/heatmap/generate?demoId=7&mapName=de_anubis&eventType=kills&team=');
        
        console.log('Response status:', response.status);
        console.log('Response headers:', response.headers);
        
        if (response.ok) {
            const contentType = response.headers.get('content-type');
            console.log('Content-Type:', contentType);
            
            if (contentType && contentType.includes('image/png')) {
                console.log('✅ Heatmap generation successful - PNG image returned');
                
                // Save the image for inspection
                const buffer = await response.arrayBuffer();
                console.log('Image size:', buffer.byteLength, 'bytes');
                
                // You can manually open the URL in a browser to see the image
                console.log('Open this URL in your browser to see the heatmap:');
                console.log('http://localhost:5163/api/heatmap/generate?demoId=7&mapName=de_anubis&eventType=kills&team=');
            } else {
                console.log('❌ Unexpected content type:', contentType);
            }
        } else {
            const errorText = await response.text();
            console.log('❌ Error:', response.status, errorText);
        }
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
    }
};

// Test different endpoints
const testEndpoints = async () => {
    const endpoints = [
        'http://localhost:5163/api/heatmap/maps',
        'http://localhost:5163/api/heatmap/generate?demoId=7&mapName=de_anubis&eventType=kills&team='
    ];
    
    for (const endpoint of endpoints) {
        console.log(`\nTesting: ${endpoint}`);
        try {
            const response = await fetch(endpoint);
            console.log('Status:', response.status);
            
            if (response.ok) {
                const contentType = response.headers.get('content-type');
                console.log('Content-Type:', contentType);
                
                if (contentType && contentType.includes('application/json')) {
                    const data = await response.json();
                    console.log('Response data:', JSON.stringify(data, null, 2));
                } else if (contentType && contentType.includes('image/png')) {
                    console.log('✅ PNG image returned');
                }
            } else {
                const errorText = await response.text();
                console.log('Error:', errorText);
            }
        } catch (error) {
            console.error('Failed:', error.message);
        }
    }
};

// If running in Node.js
if (typeof require !== 'undefined') {
    const fetch = require('node-fetch');
    testEndpoints();
} else {
    // If running in browser
    testEndpoints();
}