import { Box, Grid2, ImageList, ImageListItem, Modal, Typography } from "@mui/material";
import react, { useContext, useEffect, useState } from "react";
import Carousel from "react-material-ui-carousel";
import axios from "axios";
import ProductDescription from "./productDescription";
import Descriptions from "../components/Descriptions";
import { ProductContext } from "../Context";

export const StoreFront = () => {
    const {getConfigurationAsync} = useContext(ProductContext);
    const productApi = axios.create({
        baseURL: `/Product`
    });
    const [images, setImages] = useState([]);
    const [open,setOpen] = useState(false);
    const [background,setBackground] = useState("");

    useEffect(() => {
        getConfigurationAsync(figs => {
            if(localStorage.getItem("configId") === null && figs.configId == null) return;
            productApi.get(`images/site/${localStorage.getItem("configId")}`)
                .then(async yup => {
                    let yupReturn = [];
                    for (var im of yup.data) {
                        let img = await fetch(window.atob(im.image));
                        let blob = await img.blob();
                        im.img = URL.createObjectURL(blob);
                        if(im.name.includes("background")){
                            setBackground(im.img);
                            continue;
                        }
                        yupReturn.push(im);
                    }
                    setImages(yupReturn);
                })
                .catch(nope => console.error(nope));
        })
    }, []);

    const ImageGroups = () => {
        return <Box
            sx={{display: "flex", flexDirection: "row"}}
        >
            <img alt="first" src={images[Math.floor(Math.random() * images.length)].img} style={{ width: "auto", height: "60vh" }} />
            <img alt="second" src={images[Math.floor(Math.random() * images.length)].img} style={{ width: "auto", height: "60vh" }} />
            <img alt="third" src={images[Math.floor(Math.random() * images.length)].img} style={{ width: "auto", height: "60vh" }} />
            <img alt="fourth" src={images[Math.floor(Math.random() * images.length)].img} style={{ width: "auto", height: "60vh" }} />
        </Box>
    }

    return <Box
        sx={{
            height: "60vh",
            width: "100vw",
            margin: 0,
            padding: 0,
        }}
    >
        <Carousel
            sx={{ width: "100%", height: "60vh" }}
            cycleNavigation={true}
            autoPlay={true}
            interval={30000}
            indicatorContainerProps={{
                style: {
                    position: "absolute",
                    bottom: "1em"
                }
            }}
        >
            {
                images.map((p, i) => (
                    <div
                        key={i}
                        style={{ display: "flex", flexDirection: "row", margin: 0, padding: 0 }}
                    >
                        <ImageGroups />
                    </div>
                ))
            }
        </Carousel>
        <Descriptions 
            bckImage={background}
        />
    </Box>
}

export default StoreFront;