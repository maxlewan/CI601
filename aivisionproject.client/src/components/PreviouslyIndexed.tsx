import React, { useState, useEffect } from 'react';

interface VideoWidgetsDto {
    videoName: string;
    lastIndexed: string;
    playerWidget: string;
    insightsWidget: string;
}

function PreviouslyIndexed() {
    const [videoWidgets, setVideoWidgets] = useState<VideoWidgetsDto[]>([]);
    const [selectedVideo, setSelectedVideo] = useState<number>(0);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string>('');

    useEffect(() => {
        const fetchVideos = async () => {
            setLoading(true);
            setError('');

            try {
                const response = await fetch('https://localhost:7082/display-videos', {
                    method: 'GET'
                });

                if (!response.ok) {
                    throw new Error(`HTTP error status: ${response.status}`);
                }

                const data = await response.json();
                setVideoWidgets(data);
            } catch (error: any) {
                console.error('Failed to fetch videos:', error.message || error);
                setError('Failed to fetch videos. Please try again.');
                setVideoWidgets([]);
            } finally {
                setLoading(false);
            }
        };

        fetchVideos();
    }, []);

    function formatDate(dateString: string): string {
        const date = new Date(dateString);
        return `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getFullYear()}, ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
    }

    return (
        <div>
        <h2>Previously Indexed Videos</h2>
            {loading && <div>Loading...</div>}
            {error && <div style={{ color: 'red' }}>{error}</div>}
            {!loading && !error && (
                <div>
                    <select
                        onChange={(e) => setSelectedVideo(parseInt(e.target.value, 10))}
                        value={selectedVideo}
                    >
                        {videoWidgets.map((widget, index) => (
                            <option key={index} value={index}>
                                {widget.videoName}
                            </option>
                        ))}
                    </select>
                    {videoWidgets.length > 0 && (
                        <div>
                            <h3>{`Indexed on: ${formatDate(videoWidgets[selectedVideo].lastIndexed)}`}</h3>
                            <iframe
                                title={`${videoWidgets[selectedVideo].videoName} Player`}
                                src={videoWidgets[selectedVideo].playerWidget}
                                width="750"
                                height="600"
                                frameBorder="0"
                                allowFullScreen
                            ></iframe>
                            <iframe
                                title={`${videoWidgets[selectedVideo].videoName} Insights`}
                                src={videoWidgets[selectedVideo].insightsWidget}
                                width="350"
                                height="600"
                                frameBorder="0"
                                allowFullScreen
                            ></iframe>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}

export default PreviouslyIndexed;
