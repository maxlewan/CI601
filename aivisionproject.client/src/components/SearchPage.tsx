import React, { useState } from 'react';
import VideoSearchBox from './VideoSearchBox';

interface VideoSearchRequestDto {
    startTime?: Date;
    endTime?: Date;
}

interface VideoWidgetsDto {
    videoName: string;
    playerWidget: string;
    insightsWidget: string;
}

function SearchPage(){
    const [startTime, setStartTime] = useState<string>('');
    const [endTime, setEndTime] = useState<string>('');
    const [selectedVideo, setSelectedVideo] = useState<number>(0);
    const [videoWidgets, setVideoWidgets] = useState<VideoWidgetsDto[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string>('');

    const handleSearch = async (event: React.FormEvent) => {
        event.preventDefault();
        setLoading(true);
        setError('');

        const requestData: VideoSearchRequestDto = {
            startTime: new Date(startTime),
            endTime: new Date(endTime)
        };

        try {
            const response = await fetch('https://localhost:7082/search-display-videos', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
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

    return (
        <div>
            <form onSubmit={handleSearch}>
                <input
                    type="datetime-local"
                    value={startTime}
                    onChange={e => setStartTime(e.target.value)}
                    disabled={loading}
                />
                <input
                    type="datetime-local"
                    value={endTime}
                    onChange={e => setEndTime(e.target.value)}
                    disabled={loading}
                />
                <button type="submit" disabled={loading}>
                    {loading ? 'Loading...' : 'Find Videos'}
                </button>
            </form>
            {error && <div style={{ color: 'red' }}>{error}</div>}
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
                        <h3>{videoWidgets[selectedVideo].videoName}</h3>
                        <iframe
                            title={`${videoWidgets[selectedVideo].videoName} Player`}
                            src={videoWidgets[selectedVideo].playerWidget}
                            width="640"
                            height="450"
                            frameBorder="0"
                            allowFullScreen
                        ></iframe>
                        <iframe
                            title={`${videoWidgets[selectedVideo].videoName} Insights`}
                            src={videoWidgets[selectedVideo].insightsWidget}
                            width="350"
                            height="450"
                            frameBorder="0"
                            allowFullScreen
                        ></iframe>
                        <VideoSearchBox />
                    </div>
                )}
            </div>
        </div>
    );
}

export default SearchPage;
