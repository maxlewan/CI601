import React, { useState } from 'react';

interface SearchResult {
    documentId: string;
    best: string;
}

const VideoSearchBox: React.FC = () => {
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string>('');

    const handleQuery = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setLoading(true);
        setError('');

        try {
            const response = await fetch('https://localhost:7082/search-in-videos', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ searchQuery })
            });

            if (!response.ok) {
                throw new Error(`HTTP error status: ${response.status}`);
            }

            const data = await response.json();

            if (!data || !data.value) {
                throw new Error("Received null or invalid data");
            }
            setSearchResults(data.value);

        } catch (error: any) {
            console.error('Failed to search videos:', error.message || error);
            setError('Failed to search videos. Please try again.');
            setSearchResults([]);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ width: '1000px', height: '250px', border: '1px solid black' }}>
            <form onSubmit={handleQuery} style={{ display: 'flex', padding: '10px' }}>
                <input
                    type="text"
                    placeholder="Search... e.g. 'person running'"
                    value={searchQuery}
                    onChange={e => setSearchQuery(e.target.value)}
                    disabled={loading}
                    style={{ flex: 1, marginRight: '10px' }}
                />
                <button type="submit" disabled={loading}>
                    {loading ? 'Searching...' : 'Search'}
                </button>
            </form>
            {error && <div style={{ color: 'red' }}>{error}</div>}
            <div style={{ overflowY: 'auto', height: '180px' }}>
                {searchResults.map((result, index) => (
                    <div key={index} style={{ padding: '0px', borderBottom: '1px solid #ccc' }}>
                        <p>{result.documentId} <b>{result.best}</b></p>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default VideoSearchBox;
