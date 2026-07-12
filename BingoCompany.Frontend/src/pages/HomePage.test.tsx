import { fireEvent,render,screen } from "@testing-library/react";import { MemoryRouter,useLocation } from "react-router-dom";import { describe,expect,it } from "vitest";import { HomePage } from "./HomePage";
function Location(){return <span data-testid="location">{useLocation().pathname}</span>}
describe("HomePage",()=>{it("leva o organizador à administração",()=>{render(<MemoryRouter><HomePage/><Location/></MemoryRouter>);fireEvent.click(screen.getByRole("button",{name:"Criar evento"}));expect(screen.getByTestId("location")).toHaveTextContent("/admin")})});
